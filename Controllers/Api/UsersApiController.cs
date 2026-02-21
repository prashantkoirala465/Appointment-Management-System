using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Models;
using AppointmentSystem.Web.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Controllers.Api
{
    /// API controller for managing user accounts
    /// All operations restricted to Admin role
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Admin")]
    [Produces("application/json")]
    public class UsersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/usersapi
        /// Returns all users with their roles and menus (pending users listed first)
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.UserMenus).ThenInclude(um => um.Menu)
                .OrderByDescending(u => !u.IsApproved)
                .ThenBy(u => u.FullName)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    IsApproved = u.IsApproved,
                    CreatedAtUtc = u.CreatedAtUtc,
                    Roles = u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.RoleName).ToList(),
                    Menus = u.UserMenus.Where(um => um.Menu != null).Select(um => um.Menu!.MenuName).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        /// GET: api/usersapi/{id}
        /// Returns a single user by ID
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDto>> GetById(Guid id)
        {
            var u = await _context.Users
                .Include(x => x.UserRoles).ThenInclude(ur => ur.Role)
                .Include(x => x.UserMenus).ThenInclude(um => um.Menu)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (u == null) return NotFound(new { message = "User not found." });

            return Ok(new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Username = u.Username,
                Email = u.Email,
                IsActive = u.IsActive,
                IsApproved = u.IsApproved,
                CreatedAtUtc = u.CreatedAtUtc,
                Roles = u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.RoleName).ToList(),
                Menus = u.UserMenus.Where(um => um.Menu != null).Select(um => um.Menu!.MenuName).ToList()
            });
        }

        /// POST: api/usersapi
        /// Creates a new user with roles and menus (auto-approved since admin is creating)
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest(new { message = "Username already taken." });

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = AccountController.HashPassword(dto.Password),
                IsActive = dto.IsActive,
                IsApproved = true, // Admin-created users are automatically approved
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign roles
            foreach (var roleId in dto.RoleIds)
            {
                if (await _context.Roles.AnyAsync(r => r.Id == roleId))
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleId = roleId
                    });
                }
            }

            // Assign menus
            foreach (var menuId in dto.MenuIds)
            {
                if (await _context.Menus.AnyAsync(m => m.Id == menuId))
                {
                    _context.UserMenus.Add(new UserMenu
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        MenuId = menuId
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Reload with relationships for the response
            var result = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.UserMenus).ThenInclude(um => um.Menu)
                .FirstAsync(u => u.Id == user.Id);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserDto
            {
                Id = result.Id,
                FullName = result.FullName,
                Username = result.Username,
                Email = result.Email,
                IsActive = result.IsActive,
                IsApproved = result.IsApproved,
                CreatedAtUtc = result.CreatedAtUtc,
                Roles = result.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.RoleName).ToList(),
                Menus = result.UserMenus.Where(um => um.Menu != null).Select(um => um.Menu!.MenuName).ToList()
            });
        }

        /// POST: api/usersapi/{id}/approve
        /// Approves a pending user registration
        [HttpPost("{id}/approve")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Approve(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            user.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User '{user.Username}' approved." });
        }

        /// POST: api/usersapi/{id}/reject
        /// Rejects and deletes a pending user registration
        [HttpPost("{id}/reject")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Reject(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            // Remove related records first
            var userRoles = await _context.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
            var userMenus = await _context.UserMenus.Where(um => um.UserId == id).ToListAsync();

            _context.UserRoles.RemoveRange(userRoles);
            _context.UserMenus.RemoveRange(userMenus);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// DELETE: api/usersapi/{id}
        /// Permanently deletes a user and their role/menu assignments
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            var userRoles = await _context.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
            var userMenus = await _context.UserMenus.Where(um => um.UserId == id).ToListAsync();

            _context.UserRoles.RemoveRange(userRoles);
            _context.UserMenus.RemoveRange(userMenus);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
