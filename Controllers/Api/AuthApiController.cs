using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Models.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AppointmentSystem.Web.Controllers.Api
{
    /// REST API controller for authentication (Ananta's module)
    /// This is the entry point for any API consumer to get a JWT token
    /// Three endpoints: login (get token), logout (clear cookie), me (who am I?)
    /// Unlike the MVC AccountController which uses cookies, this one returns JWT tokens
    /// for stateless API authentication (used by mobile apps, Postman, SPAs, etc.)
    [ApiController]                   // enables automatic model validation + JSON responses
    [Route("api/[controller]")]       // base URL: /api/authapi
    [Produces("application/json")]    // all responses are JSON
    public class AuthApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;     // database access
        private readonly IConfiguration _configuration;     // reads JWT settings from appsettings.json

        // both dependencies injected by ASP.NET Core's DI container
        public AuthApiController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// POST: api/authapi/login
        /// Authenticates a user and returns a JWT token for API access
        /// Also creates a session cookie for browser-based access
        [HttpPost("login")]                              // POST /api/authapi/login
        [ProducesResponseType(typeof(LoginResponseDto), 200)] // Swagger: documents success shape
        [ProducesResponseType(401)]                           // Swagger: documents 401 on bad creds
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // [ApiController] already validates the model, but this is an extra safety check
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // look up user by username, also load their roles
            // we only look for active users (IsActive == true)
            var user = await _context.Users
                .Include(u => u.UserRoles)        // load junction table entries
                    .ThenInclude(ur => ur.Role)    // load the actual Role name
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.IsActive);

            // check password — we hash the input and compare to the stored hash
            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid username or password." });

            // staff who registered but haven't been approved yet can't log in
            if (!user.IsApproved)
                return Unauthorized(new { message = "Your account is pending admin approval." });

            // --- BUILD CLAIMS ---
            // claims are key-value pairs that get embedded into the JWT token
            // the [Authorize] middleware reads these to know who the user is and what roles they have
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  // unique user ID
                new Claim(ClaimTypes.Name, user.Username),                 // username
                new Claim("FullName", user.FullName)                       // display name (custom claim)
            };

            // add a Role claim for each role the user has
            // these are what [Authorize(Roles = "Admin")] checks against
            var roleNames = new List<string>();
            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
                    roleNames.Add(userRole.Role.RoleName);
                }
            }

            // --- COOKIE SIGN-IN ---
            // we also create a cookie session so that Swagger UI and browser testing work
            // without having to manually paste the JWT token each time
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // --- JWT TOKEN GENERATION ---
            // generate a signed JWT token that API consumers send in the Authorization header
            var token = GenerateJwtToken(claims);

            // return the token + user info in the response
            return Ok(new LoginResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Roles = roleNames,
                Token = token.Token,
                ExpiresAt = token.ExpiresAt,
                Message = "Login successful. Use the token in the Authorization header as: Bearer <token>"
            });
        }

        /// POST: api/authapi/logout
        /// Signs out the current user and clears the session cookie
        /// Note: JWT tokens cannot be revoked server-side — they expire naturally
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully. Note: JWT tokens remain valid until expiry." });
        }

        /// GET: api/authapi/me
        /// Returns the current authenticated user's info
        /// Works with both cookie and JWT authentication
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(401)]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name);
            var fullName = User.FindFirstValue("FullName");
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new LoginResponseDto
            {
                UserId = Guid.TryParse(userId, out var id) ? id : Guid.Empty,
                Username = username ?? "",
                FullName = fullName ?? "",
                Roles = roles,
                Message = "Authenticated."
            });
        }

        /// Generates a signed JWT token containing the user's claims
        /// This is the heart of JWT auth — it creates a tamper-proof token
        /// that encodes who the user is and what roles they have
        private (string Token, DateTime ExpiresAt) GenerateJwtToken(List<Claim> claims)
        {
            // read the secret signing key from appsettings.json
            // this MUST match what's configured in Program.cs for token validation
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Authentication:Jwt:Key"]!));

            // HMAC-SHA256 is the algorithm used to sign the token
            // if someone tampers with the token payload, the signature won't match
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // token expires after this many minutes (default 60 if not configured)
            var expiryMinutes = int.Parse(_configuration["Authentication:Jwt:ExpiryInMinutes"] ?? "60");
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            // assemble the JWT with issuer, audience, claims, expiry, and signing credentials
            var token = new JwtSecurityToken(
                issuer: _configuration["Authentication:Jwt:Issuer"],      // who issued this token
                audience: _configuration["Authentication:Jwt:Audience"],  // who can use this token
                claims: claims,                                           // user data inside the token
                expires: expiresAt,                                       // when it stops working
                signingCredentials: credentials);                         // the cryptographic signature

            // serialize the token to a string (the "eyJhbGci..." format you see in headers)
            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        /// Simple password verification: hash the input and compare to what's stored
        /// We reuse the same SHA-256 hasher from AccountController
        private bool VerifyPassword(string password, string storedHash)
        {
            return AccountController.HashPassword(password) == storedHash;
        }
    }
}
