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
    /// REST API controller for managing staff members (Anupam's module)
    /// Staff are the people who appointments get assigned to (doctors, therapists, etc.)
    /// Read endpoints (GET) are open to any authenticated user
    /// Write endpoints (POST/PUT/DELETE) are restricted to Admins only
    [ApiController]                   // enables automatic model validation
    [Route("api/[controller]")]       // base: /api/staffsapi
    [Authorize]                       // must be logged in for all endpoints
    [Produces("application/json")]    // JSON responses
    public class StaffsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // EF Core database access

        public StaffsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/staffsapi
        /// Returns a list of all staff members with a count of how many appointments each has
        /// Any authenticated user can call this (used when creating an appointment to pick a staff)
        [HttpGet]
        [ProducesResponseType(typeof(List<StaffDto>), 200)]
        public async Task<ActionResult<List<StaffDto>>> GetAll()
        {
            var staffs = await _context.Staffs
                .Include(s => s.Appointments)          // load appointments to count them
                .OrderBy(s => s.FullName)               // alphabetical order
                .Select(s => new StaffDto               // project into a clean DTO
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber,
                    Specialty = s.Specialty,             // e.g., "Cardiology", "Pediatrics"
                    IsActive = s.IsActive,
                    AppointmentCount = s.Appointments.Count  // handy stat for the UI
                })
                .ToListAsync();

            return Ok(staffs);
        }

        /// GET: api/staffsapi/{id}
        /// Returns a single staff member by ID
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(StaffDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<StaffDto>> GetById(Guid id)
        {
            var s = await _context.Staffs
                .Include(x => x.Appointments)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null) return NotFound(new { message = "Staff member not found." });

            return Ok(new StaffDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Specialty = s.Specialty,
                IsActive = s.IsActive,
                AppointmentCount = s.Appointments.Count
            });
        }

        /// POST: api/staffsapi
        /// Creates a new staff member (Admin only)
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [ProducesResponseType(typeof(StaffDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<StaffDto>> Create([FromBody] StaffCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Specialty = dto.Specialty,
                IsActive = dto.IsActive
            };

            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();

            var result = new StaffDto
            {
                Id = staff.Id,
                FullName = staff.FullName,
                Email = staff.Email,
                PhoneNumber = staff.PhoneNumber,
                Specialty = staff.Specialty,
                IsActive = staff.IsActive,
                AppointmentCount = 0
            };

            return CreatedAtAction(nameof(GetById), new { id = staff.Id }, result);
        }

        /// PUT: api/staffsapi/{id}
        /// Updates an existing staff member (Admin only)
        [HttpPut("{id}")]
        [Authorize(Policy = "Admin")]
        [ProducesResponseType(typeof(StaffDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<StaffDto>> Update(Guid id, [FromBody] StaffCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var staff = await _context.Staffs
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null) return NotFound(new { message = "Staff member not found." });

            staff.FullName = dto.FullName;
            staff.Email = dto.Email;
            staff.PhoneNumber = dto.PhoneNumber;
            staff.Specialty = dto.Specialty;
            staff.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new StaffDto
            {
                Id = staff.Id,
                FullName = staff.FullName,
                Email = staff.Email,
                PhoneNumber = staff.PhoneNumber,
                Specialty = staff.Specialty,
                IsActive = staff.IsActive,
                AppointmentCount = staff.Appointments.Count
            });
        }

        /// DELETE: api/staffsapi/{id}
        /// Removes a staff member (Admin only)
        /// Smart delete logic:
        ///   - If the staff has appointments linked to them, we can't hard-delete
        ///     (FK constraint would break), so we soft-delete by setting IsActive = false
        ///   - If no appointments exist, we can safely hard-delete the record
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]          // only admins can delete staff
        [ProducesResponseType(204)]             // 204 = hard deleted
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            // load staff with appointments to check if any exist
            var staff = await _context.Staffs
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null) return NotFound(new { message = "Staff member not found." });

            if (staff.Appointments.Any())
            {
                // soft delete — can't remove because appointments reference this staff
                // instead, deactivate them so they don't show up in new appointment forms
                staff.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Staff deactivated (has linked appointments)." });
            }

            // hard delete — no appointments reference this staff, safe to remove entirely
            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
            return NoContent();   // 204 = gone
        }
    }
}
