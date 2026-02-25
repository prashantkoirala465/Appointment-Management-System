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
    /// REST API controller for appointments (Prashant's module)
    /// Provides full CRUD operations as a RESTful resource:
    ///   GET    /api/appointmentsapi       → list all appointments
    ///   GET    /api/appointmentsapi/{id}   → get one appointment
    ///   POST   /api/appointmentsapi        → create new appointment
    ///   PUT    /api/appointmentsapi/{id}   → update an appointment
    ///   DELETE /api/appointmentsapi/{id}   → delete an appointment
    /// All endpoints require authentication (cookie or JWT Bearer token)
    [ApiController]                   // enables automatic model validation + problem details
    [Route("api/[controller]")]       // base route: /api/appointmentsapi
    [Authorize]                       // must be logged in (any role)
    [Produces("application/json")]    // tells Swagger all responses are JSON
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // EF Core database context

        public AppointmentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/appointmentsapi
        /// Returns ALL appointments, newest first, with staff member info included
        /// This is the main listing endpoint — the frontend table or a mobile app calls this
        [HttpGet]
        [ProducesResponseType(typeof(List<AppointmentDto>), 200)] // Swagger: documents the response shape
        public async Task<ActionResult<List<AppointmentDto>>> GetAll()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Staff)                    // join with Staff table to get the staff name
                .OrderByDescending(a => a.StartTime)      // most recent appointments first
                .Select(a => new AppointmentDto            // project into a DTO (data transfer object)
                {                                          // DTOs control exactly what data we expose
                    Id = a.Id,
                    StaffId = a.StaffId,
                    StaffName = a.Staff != null ? a.Staff.FullName : "",  // null-safe staff name
                    ClientName = a.ClientName,
                    ClientEmail = a.ClientEmail,
                    ClientPhone = a.ClientPhone,
                    StartTime = a.StartTime,
                    DurationMinutes = a.DurationMinutes,
                    Status = a.Status,                     // "Scheduled", "Completed", or "Cancelled"
                    Notes = a.Notes,
                    CreatedAtUtc = a.CreatedAtUtc,
                    UpdatedAtUtc = a.UpdatedAtUtc
                })
                .ToListAsync();                            // execute the SQL query

            return Ok(appointments);                       // 200 OK with JSON array
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
        /// Creates a new appointment — the client sends JSON with staff, client info, time, etc.
        /// Returns 201 Created with a Location header pointing to the new resource
        [HttpPost]
        [ProducesResponseType(typeof(AppointmentDto), 201)]  // 201 = resource created
        [ProducesResponseType(400)]                           // 400 = validation error
        public async Task<ActionResult<AppointmentDto>> Create([FromBody] AppointmentCreateDto dto)
        {
            // [ApiController] handles most validation, but we double-check
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // make sure the referenced staff member actually exists in our database
            var staff = await _context.Staffs.FindAsync(dto.StaffId);
            if (staff == null)
                return BadRequest(new { message = "Staff member not found." });

            // build the new Appointment entity from the incoming DTO
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),              // generate unique primary key
                StaffId = dto.StaffId,             // FK to the staff member
                ClientName = dto.ClientName,
                ClientEmail = dto.ClientEmail,
                ClientPhone = dto.ClientPhone,
                StartTime = dto.StartTime,
                DurationMinutes = dto.DurationMinutes,
                Status = dto.Status,               // "Scheduled", "Completed", or "Cancelled"
                Notes = dto.Notes,
                CreatedAtUtc = DateTime.UtcNow     // timestamp when it was created
            };

            _context.Appointments.Add(appointment);   // queue for insertion
            await _context.SaveChangesAsync();        // write to SQLite

            // build the response DTO (includes staff name which wasn't in the create DTO)
            var result = new AppointmentDto
            {
                Id = appointment.Id,
                StaffId = appointment.StaffId,
                StaffName = staff.FullName,           // we looked this up earlier
                ClientName = appointment.ClientName,
                ClientEmail = appointment.ClientEmail,
                ClientPhone = appointment.ClientPhone,
                StartTime = appointment.StartTime,
                DurationMinutes = appointment.DurationMinutes,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedAtUtc = appointment.CreatedAtUtc
            };

            // REST best practice: return 201 with Location header
            // Location: /api/appointmentsapi/{id}
            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, result);
        }

        /// PUT: api/appointmentsapi/{id}
        /// Updates an existing appointment — full replacement (not partial/PATCH)
        /// The client sends all fields, even the ones that didn't change
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AppointmentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AppointmentDto>> Update(Guid id, [FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // find the existing appointment by ID, include staff info
            var appointment = await _context.Appointments
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(a => a.Id == id);

            // can't update what doesn't exist
            if (appointment == null) return NotFound(new { message = "Appointment not found." });

            // if the staff assignment is changing, verify the new staff exists
            var staff = await _context.Staffs.FindAsync(dto.StaffId);
            if (staff == null)
                return BadRequest(new { message = "Staff member not found." });

            // overwrite all the fields with the new values from the DTO
            appointment.StaffId = dto.StaffId;
            appointment.ClientName = dto.ClientName;
            appointment.ClientEmail = dto.ClientEmail;
            appointment.ClientPhone = dto.ClientPhone;
            appointment.StartTime = dto.StartTime;
            appointment.DurationMinutes = dto.DurationMinutes;
            appointment.Status = dto.Status;
            appointment.Notes = dto.Notes;
            appointment.UpdatedAtUtc = DateTime.UtcNow;   // track when it was last modified

            await _context.SaveChangesAsync();             // persist to database

            // return the updated appointment as a DTO
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
        /// Permanently removes an appointment from the database
        /// Returns 204 No Content on success (REST convention for deletes)
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]    // 204 = deleted successfully, no body
        [ProducesResponseType(404)]    // 404 = appointment with this ID doesn't exist
        public async Task<IActionResult> Delete(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound(new { message = "Appointment not found." });

            _context.Appointments.Remove(appointment);    // mark for deletion
            await _context.SaveChangesAsync();            // execute the DELETE SQL

            return NoContent();                           // 204 — done, nothing to return
        }
    }
}
