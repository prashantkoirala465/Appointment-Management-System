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
    /// API controller for managing roles
    /// All operations restricted to Admin role
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Admin")]
    [Produces("application/json")]
    public class RolesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/rolesapi
        /// Returns all roles with user counts
        [HttpGet]
        [ProducesResponseType(typeof(List<RoleDto>), 200)]
        public async Task<ActionResult<List<RoleDto>>> GetAll()
        {
            var roles = await _context.Roles
                .Include(r => r.UserRoles)
                .OrderBy(r => r.RoleName)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    UserCount = r.UserRoles.Count
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
        /// Creates a new role
        [HttpPost]
        [ProducesResponseType(typeof(RoleDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<RoleDto>> Create([FromBody] RoleCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Roles.AnyAsync(r => r.RoleName == dto.RoleName))
                return BadRequest(new { message = "A role with this name already exists." });

            var role = new Role
            {
                Id = Guid.NewGuid(),
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
                UserCount = 0
            };

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
        /// Permanently deletes a role and its user assignments
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound(new { message = "Role not found." });

            var userRoles = await _context.UserRoles.Where(ur => ur.RoleId == id).ToListAsync();
            _context.UserRoles.RemoveRange(userRoles);
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
