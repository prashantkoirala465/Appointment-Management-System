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
    /// API controller for managing appointments
    /// Provides full CRUD operations for the appointment resource
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/appointmentsapi
        /// Returns all appointments with their assigned staff member names
        [HttpGet]
        [ProducesResponseType(typeof(List<AppointmentDto>), 200)]
        public async Task<ActionResult<List<AppointmentDto>>> GetAll()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Staff)
                .OrderByDescending(a => a.StartTime)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    StaffId = a.StaffId,
                    StaffName = a.Staff != null ? a.Staff.FullName : "",
                    ClientName = a.ClientName,
                    ClientEmail = a.ClientEmail,
                    ClientPhone = a.ClientPhone,
                    StartTime = a.StartTime,
                    DurationMinutes = a.DurationMinutes,
                    Status = a.Status,
                    Notes = a.Notes,
                    CreatedAtUtc = a.CreatedAtUtc,
                    UpdatedAtUtc = a.UpdatedAtUtc
                })
                .ToListAsync();

            return Ok(appointments);
        }

        /// GET: api/appointmentsapi/{id}
        /// Returns a single appointment by its ID
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AppointmentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AppointmentDto>> GetById(Guid id)
        {
            var a = await _context.Appointments
                .Include(x => x.Staff)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a == null) return NotFound(new { message = "Appointment not found." });

            return Ok(new AppointmentDto
            {
                Id = a.Id,
                StaffId = a.StaffId,
                StaffName = a.Staff?.FullName ?? "",
                ClientName = a.ClientName,
                ClientEmail = a.ClientEmail,
                ClientPhone = a.ClientPhone,
                StartTime = a.StartTime,
                DurationMinutes = a.DurationMinutes,
                Status = a.Status,
                Notes = a.Notes,
                CreatedAtUtc = a.CreatedAtUtc,
                UpdatedAtUtc = a.UpdatedAtUtc
            });
        }

        /// POST: api/appointmentsapi
        /// Creates a new appointment
        [HttpPost]
        [ProducesResponseType(typeof(AppointmentDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AppointmentDto>> Create([FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Verify the staff member exists
            var staff = await _context.Staffs.FindAsync(dto.StaffId);
            if (staff == null)
                return BadRequest(new { message = "Staff member not found." });

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                StaffId = dto.StaffId,
                ClientName = dto.ClientName,
                ClientEmail = dto.ClientEmail,
                ClientPhone = dto.ClientPhone,
                StartTime = dto.StartTime,
                DurationMinutes = dto.DurationMinutes,
                Status = dto.Status,
                Notes = dto.Notes,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var result = new AppointmentDto
            {
                Id = appointment.Id,
                StaffId = appointment.StaffId,
                StaffName = staff.FullName,
                ClientName = appointment.ClientName,
                ClientEmail = appointment.ClientEmail,
                ClientPhone = appointment.ClientPhone,
                StartTime = appointment.StartTime,
                DurationMinutes = appointment.DurationMinutes,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedAtUtc = appointment.CreatedAtUtc
            };

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, result);
        }

        /// PUT: api/appointmentsapi/{id}
        /// Updates an existing appointment
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AppointmentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AppointmentDto>> Update(Guid id, [FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var appointment = await _context.Appointments
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound(new { message = "Appointment not found." });

            var staff = await _context.Staffs.FindAsync(dto.StaffId);
            if (staff == null)
                return BadRequest(new { message = "Staff member not found." });

            appointment.StaffId = dto.StaffId;
            appointment.ClientName = dto.ClientName;
            appointment.ClientEmail = dto.ClientEmail;
            appointment.ClientPhone = dto.ClientPhone;
            appointment.StartTime = dto.StartTime;
            appointment.DurationMinutes = dto.DurationMinutes;
            appointment.Status = dto.Status;
            appointment.Notes = dto.Notes;
            appointment.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new AppointmentDto
            {
                Id = appointment.Id,
                StaffId = appointment.StaffId,
                StaffName = staff.FullName,
                ClientName = appointment.ClientName,
                ClientEmail = appointment.ClientEmail,
                ClientPhone = appointment.ClientPhone,
                StartTime = appointment.StartTime,
                DurationMinutes = appointment.DurationMinutes,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedAtUtc = appointment.CreatedAtUtc,
                UpdatedAtUtc = appointment.UpdatedAtUtc
            });
        }

        /// DELETE: api/appointmentsapi/{id}
        /// Permanently deletes an appointment
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound(new { message = "Appointment not found." });

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
