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
    /// REST API controller for managing roles (Ananta's module)
    /// Roles are things like "Admin", "Staff", "Doctor" — they control what a user can access
    /// All endpoints require Admin access since only admins should manage roles
    /// Supports full CRUD: list, get by ID, create, update, delete
    [ApiController]                       // enables automatic validation + JSON errors
    [Route("api/[controller]")]           // base: /api/rolesapi
    [Authorize(Policy = "Admin")]         // every endpoint here requires Admin role
    [Produces("application/json")]        // all responses are JSON
    public class RolesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // database access

        public RolesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/rolesapi
        /// Returns all roles with a count of how many users are assigned to each
        [HttpGet]
        [ProducesResponseType(typeof(List<RoleDto>), 200)]
        public async Task<ActionResult<List<RoleDto>>> GetAll()
        {
            var roles = await _context.Roles
                .Include(r => r.UserRoles)                // load junction table for counting
                .OrderBy(r => r.RoleName)                  // alphabetical order
                .Select(r => new RoleDto                   // project into DTO
                {
                    Id = r.Id,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    UserCount = r.UserRoles.Count          // how many users have this role
                })
                .ToListAsync();

            return Ok(roles);
        }

        /// GET: api/rolesapi/{id}
        /// Returns a single role by ID
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RoleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RoleDto>> GetById(Guid id)
        {
            var r = await _context.Roles
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null) return NotFound(new { message = "Role not found." });

            return Ok(new RoleDto
            {
                Id = r.Id,
                RoleName = r.RoleName,
                Description = r.Description,
                IsActive = r.IsActive,
                UserCount = r.UserRoles.Count
            });
        }

        /// POST: api/rolesapi
        /// Creates a new role — first checks that no other role uses the same name
        [HttpPost]
        [ProducesResponseType(typeof(RoleDto), 201)]    // 201 = created
        [ProducesResponseType(400)]                      // 400 = duplicate name or validation error
        public async Task<ActionResult<RoleDto>> Create([FromBody] RoleCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // enforce unique role names — no point having two "Admin" roles
            if (await _context.Roles.AnyAsync(r => r.RoleName == dto.RoleName))
                return BadRequest(new { message = "A role with this name already exists." });

            var role = new Role
            {
                Id = Guid.NewGuid(),           // generate unique primary key
                RoleName = dto.RoleName,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var result = new RoleDto
            {
                Id = role.Id,
                RoleName = role.RoleName,
                Description = role.Description,
                IsActive = role.IsActive,
                UserCount = 0                  // brand new role has no users yet
            };

            // REST convention: return 201 with Location header
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, result);
        }

        /// PUT: api/rolesapi/{id}
        /// Updates an existing role
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RoleDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RoleDto>> Update(Guid id, [FromBody] RoleCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null) return NotFound(new { message = "Role not found." });

            if (await _context.Roles.AnyAsync(r => r.RoleName == dto.RoleName && r.Id != id))
                return BadRequest(new { message = "A role with this name already exists." });

            role.RoleName = dto.RoleName;
            role.Description = dto.Description;
            role.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new RoleDto
            {
                Id = role.Id,
                RoleName = role.RoleName,
                Description = role.Description,
                IsActive = role.IsActive,
                UserCount = role.UserRoles.Count
            });
        }

        /// DELETE: api/rolesapi/{id}
        /// Permanently deletes a role and removes all user-role assignments for it
        /// Must clean up junction table first due to FK constraint
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]   // 204 = deleted
        [ProducesResponseType(404)]   // 404 = role doesn't exist
        public async Task<IActionResult> Delete(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound(new { message = "Role not found." });

            // remove all user-role junction records that reference this role
            // if we skip this, SQLite will reject the delete due to FK constraint
            var userRoles = await _context.UserRoles.Where(ur => ur.RoleId == id).ToListAsync();
            _context.UserRoles.RemoveRange(userRoles);

            // now safely delete the role itself
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();  // 204 = done
        }
    }
}
