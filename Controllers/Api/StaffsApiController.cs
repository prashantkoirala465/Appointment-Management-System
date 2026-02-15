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
    /// API controller for managing staff members
    /// Read access for any authenticated user; create/update/delete restricted to Admin role
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class StaffsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StaffsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/staffsapi
        /// Returns all staff members with their appointment counts
        [HttpGet]
        [ProducesResponseType(typeof(List<StaffDto>), 200)]
        public async Task<ActionResult<List<StaffDto>>> GetAll()
        {
            var staffs = await _context.Staffs
                .Include(s => s.Appointments)
                .OrderBy(s => s.FullName)
                .Select(s => new StaffDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber,
                    Specialty = s.Specialty,
                    IsActive = s.IsActive,
                    AppointmentCount = s.Appointments.Count
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        /// Deletes a staff member (Admin only)
        /// Soft-deletes if the staff has appointments, hard-deletes otherwise
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var staff = await _context.Staffs
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null) return NotFound(new { message = "Staff member not found." });

            if (staff.Appointments.Any())
            {
                // Soft delete — deactivate instead of removing
                staff.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Staff deactivated (has linked appointments)." });
            }

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
