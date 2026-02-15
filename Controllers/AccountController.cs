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
    /// This controller handles user authentication - logging in and logging out
    /// It uses cookie-based authentication, which is a simple and effective approach for MVC apps
    /// When a user logs in, a secure cookie is set in their browser to track their session
    public class AccountController : Controller
    {
        // Our database context - used to look up users and verify credentials
        private readonly ApplicationDbContext _context;

        // Constructor: ASP.NET Core injects the database context automatically
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        // Shows the login form to the user
        // If the user is already logged in, redirect them to the homepage
        public IActionResult Login()
        {
            // If user is already authenticated, no need to show login page
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Account/Login
        // Processes the login form submission
        // Validates the username and password against the database
        [HttpPost]
        [ValidateAntiForgeryToken] // Security: prevents cross-site request forgery attacks
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Check if the form data is valid (required fields filled, etc.)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Look up the user by username in the database
            // We also load their roles through the UserRole junction table
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive);

            // If user not found or password doesn't match, show an error
            // We use a generic message to avoid revealing whether the username exists (security best practice)
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            // Authentication successful! Now create the claims for the cookie
            // Claims are pieces of information about the user that get stored in the cookie
            var claims = new List<Claim>
            {
                // Store the user's unique ID so we can look them up later
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // Store the username for display purposes
                new Claim(ClaimTypes.Name, user.Username),
                // Store the full name for display in the UI
                new Claim("FullName", user.FullName)
            };

            // Add each of the user's roles as a claim
            // This lets us use [Authorize(Roles = "Admin")] on controllers and actions
            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
                }
            }

            // Create a claims identity with the cookie authentication scheme
            // This bundles all claims together into a single identity
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Create a claims principal (represents the authenticated user)
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Actually sign the user in by creating the authentication cookie
            // The cookie will be sent with every subsequent request so we know who the user is
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal);

            // Success! Redirect to the homepage
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        // Logs the user out by deleting their authentication cookie
        [HttpPost]
        [ValidateAntiForgeryToken] // Security: prevents CSRF attacks
        public async Task<IActionResult> Logout()
        {
            // Remove the authentication cookie - this effectively logs the user out
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to the login page after logging out
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/AccessDenied
        // Shows a friendly page when a user tries to access something they don't have permission for
        // For example, if a "Staff" user tries to access an "Admin-only" page
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper method to verify a password against its hash
        // We use a simple hash comparison here
        // In a production app, you'd use BCrypt or similar, but this keeps things simple for learning
        private bool VerifyPassword(string password, string storedHash)
        {
            // Hash the provided password and compare it to the stored hash
            return HashPassword(password) == storedHash;
        }

        // Helper method to hash a password
        // Uses SHA256 for simplicity in this educational project
        // In production, you'd want to use BCrypt, Argon2, or PBKDF2 instead
        public static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
