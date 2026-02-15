using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
