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
    /// API controller for authentication
    /// Provides login (returns JWT token), logout, and current user info endpoints
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthApiController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// POST: api/authapi/login
        /// Authenticates a user and returns a JWT token for API access
        /// Also creates a session cookie for browser-based access
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.IsActive);

            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid username or password." });

            if (!user.IsApproved)
                return Unauthorized(new { message = "Your account is pending admin approval." });

            // Build claims shared by both cookie and JWT auth
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName)
            };

            var roleNames = new List<string>();
            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
                    roleNames.Add(userRole.Role.RoleName);
                }
            }

            // Also sign in with cookie so browser-based API testing (Swagger UI) works
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // Generate JWT token for API consumers (mobile apps, SPAs, Postman, etc.)
            var token = GenerateJwtToken(claims);

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
        /// The token includes the user's ID, username, name, and roles
        private (string Token, DateTime ExpiresAt) GenerateJwtToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Authentication:Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = int.Parse(_configuration["Authentication:Jwt:ExpiryInMinutes"] ?? "60");
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["Authentication:Jwt:Issuer"],
                audience: _configuration["Authentication:Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return AccountController.HashPassword(password) == storedHash;
        }
    }
}
