using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Controllers
{
    /// Handles authentication — login, registration (staff only), and logout
    /// Staff must be approved by the admin before they can log in
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive);

            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            // Check if the user has been approved by an admin
            if (!user.IsApproved)
            {
                ModelState.AddModelError(string.Empty, "Your account is pending approval. Please wait for the administrator to verify your registration.");
                return View(model);
            }

            // Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName)
            };

            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        // Staff registration — not for admin accounts
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Register
        // Creates a new staff account with IsApproved = false
        // The admin must approve it before the user can log in
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            // Create user — NOT approved yet, must wait for admin verification
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = model.FullName,
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                IsActive = true,
                IsApproved = false, // Pending admin approval
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign the "Staff" role automatically
            var staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Staff");
            if (staffRole != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = staffRole.Id
                });
            }

            // Give them the Appointments menu by default (they can get more after approval)
            var appointmentsMenu = await _context.Menus.FirstOrDefaultAsync(m => m.MenuName == "Appointments");
            if (appointmentsMenu != null)
            {
                _context.UserMenus.Add(new UserMenu
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    MenuId = appointmentsMenu.Id
                });
            }

            await _context.SaveChangesAsync();

            // Don't sign them in — show a pending approval page instead
            return RedirectToAction(nameof(RegistrationPending));
        }

        // GET: /Account/RegistrationPending
        // Shown after a staff member registers — tells them to wait for admin approval
        public IActionResult RegistrationPending()
        {
            return View();
        }

        // GET: /Account/GoogleLogin
        // Initiates the Google OAuth flow by redirecting the user to Google's sign-in page
        // After the user authenticates with Google, they are redirected back to GoogleCallback
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            // Tell ASP.NET Core to challenge with Google and redirect to our callback
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback))
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // GET: /Account/GoogleCallback
        // This is called after Google redirects the user back to our app
        // We extract the user's info from the Google claims and either sign them in
        // or create a new pending account
        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            // Read the external login info that Google sent back
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (result?.Principal == null)
                return RedirectToAction(nameof(Login));

            // Extract the user's Google profile information from the claims
            var googleEmail = result.Principal.FindFirstValue(ClaimTypes.Email);
            var googleName = result.Principal.FindFirstValue(ClaimTypes.Name) ?? "Google User";

            if (string.IsNullOrEmpty(googleEmail))
            {
                ModelState.AddModelError(string.Empty, "Could not retrieve email from Google.");
                return View(nameof(Login), new LoginViewModel());
            }

            // Check if a user with this email already exists in our database
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == googleEmail);

            if (user == null)
            {
                // First time Google sign-in — create a new staff account (pending approval)
                // Generate a unique username from the email prefix
                var baseUsername = googleEmail.Split('@')[0];
                var username = baseUsername;
                var counter = 1;
                while (await _context.Users.AnyAsync(u => u.Username == username))
                {
                    username = $"{baseUsername}{counter++}";
                }

                user = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = googleName,
                    Username = username,
                    Email = googleEmail,
                    // Google users don't have a local password — store a random hash
                    PasswordHash = HashPassword(Guid.NewGuid().ToString()),
                    IsActive = true,
                    IsApproved = false, // Must be approved by admin, same as manual registration
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Assign the Staff role
                var staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Staff");
                if (staffRole != null)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleId = staffRole.Id
                    });
                }

                // Give them the Appointments menu by default
                var appointmentsMenu = await _context.Menus.FirstOrDefaultAsync(m => m.MenuName == "Appointments");
                if (appointmentsMenu != null)
                {
                    _context.UserMenus.Add(new UserMenu
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        MenuId = appointmentsMenu.Id
                    });
                }

                await _context.SaveChangesAsync();

                // New Google user must wait for admin approval
                return RedirectToAction(nameof(RegistrationPending));
            }

            // User exists — check if active and approved
            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account has been deactivated.");
                return View(nameof(Login), new LoginViewModel());
            }

            if (!user.IsApproved)
            {
                return RedirectToAction(nameof(RegistrationPending));
            }

            // User is approved — sign them in with our cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName)
            };

            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        public static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
