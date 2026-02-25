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
    /// REST API controller for managing user accounts (Ananta's module)
    /// This is the API equivalent of the MVC UsersController
    /// All operations are restricted to admins — regular users can't manage other users
    /// Supports: list, get, create (with roles/menus), approve, reject, delete
    [ApiController]                       // automatic model validation + JSON error responses
    [Route("api/[controller]")]           // base: /api/usersapi
    [Authorize(Policy = "Admin")]         // only admins can access ANY endpoint here
    [Produces("application/json")]        // all responses are JSON
    public class UsersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // EF Core database context

        public UsersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/usersapi
        /// Returns ALL users with their assigned role and menu names
        /// Pending (unapproved) users float to the top of the list
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)    // load roles through junction
                .Include(u => u.UserMenus).ThenInclude(um => um.Menu)    // load menus through junction
                .OrderByDescending(u => !u.IsApproved)                   // pending approvals first
                .ThenBy(u => u.FullName)                                  // then alphabetical
                .Select(u => new UserDto                                  // project into DTO
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    IsApproved = u.IsApproved,
                    CreatedAtUtc = u.CreatedAtUtc,
                    // flatten roles into a simple list of names (e.g., ["Admin", "Staff"])
                    Roles = u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.RoleName).ToList(),
                    // flatten menus into a list of names (e.g., ["Dashboard", "Appointments"])
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
        /// Creates a new user with optional role and menu assignments
        /// The client sends a JSON body with user info + arrays of role/menu GUIDs to assign
        /// User is auto-approved since an admin is creating them
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), 201)]   // 201 = created
        [ProducesResponseType(400)]                     // 400 = validation error or duplicate username
        public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // check for duplicate username before creating
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest(new { message = "Username already taken." });

            // build the user entity from the DTO
            var user = new User
            {
                Id = Guid.NewGuid(),                                     // unique primary key
                FullName = dto.FullName,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = AccountController.HashPassword(dto.Password), // SHA-256 hash
                IsActive = dto.IsActive,
                IsApproved = true,              // admin-created = skip the approval step
                CreatedAtUtc = DateTime.UtcNow  // record creation timestamp
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();  // save so the user has an ID for junction records

            // create junction records for each role ID that was sent
            // we verify each role exists first to avoid FK violations
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

            // same pattern for menu assignments
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

            await _context.SaveChangesAsync();  // save all junction records

            // reload the user with relationships so we can return role/menu names in the response
            var result = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.UserMenus).ThenInclude(um => um.Menu)
                .FirstAsync(u => u.Id == user.Id);

            // return 201 Created with Location header pointing to GET /api/usersapi/{id}
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
        /// Flips the IsApproved flag so a pending staff member can log in
        /// This is the API equivalent of the MVC Approve action
        [HttpPost("{id}/approve")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Approve(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            user.IsApproved = true;            // flip the approval flag
            await _context.SaveChangesAsync(); // persist the change

            return Ok(new { message = $"User '{user.Username}' approved." });
        }

        /// POST: api/usersapi/{id}/reject
        /// Rejects a pending registration by deleting the user entirely
        /// Also cleans up any role/menu assignments (FK constraint)
        [HttpPost("{id}/reject")]
        [ProducesResponseType(204)]   // 204 = user deleted
        [ProducesResponseType(404)]
        public async Task<IActionResult> Reject(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            // load and remove junction table records before deleting the user
            var userRoles = await _context.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
            var userMenus = await _context.UserMenus.Where(um => um.UserId == id).ToListAsync();

            _context.UserRoles.RemoveRange(userRoles);   // delete role assignments
            _context.UserMenus.RemoveRange(userMenus);   // delete menu assignments
            _context.Users.Remove(user);                  // delete the user
            await _context.SaveChangesAsync();

            return NoContent();  // 204 = gone
        }

        /// DELETE: api/usersapi/{id}
        /// Permanently deletes a user account and all their junction table records
        /// Same cleanup pattern as Reject but this is for existing (approved) users too
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            // must delete junction records first due to FK constraints
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
