using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Controllers.Api
{
    /// REST API controller for dashboard statistics (Anupam's module)
    /// Returns aggregated counts and recent appointments for the dashboard overview
    /// This is a read-only controller — only one GET endpoint that returns everything at once
    [ApiController]                   // enables automatic model validation
    [Route("api/[controller]")]       // base: /api/dashboardapi
    [Authorize]                       // must be logged in
    [Produces("application/json")]    // JSON responses
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // EF Core database access

        public DashboardApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/dashboardapi
        /// Returns a single JSON object packed with all the dashboard stats:
        ///   - appointment counts (total, by status, today's count)
        ///   - staff counts (total and active)
        ///   - user counts (total, active, pending approvals)
        ///   - 5 most recently created appointments
        /// Each count runs as a separate COUNT() SQL query against the database
        [HttpGet]
        [ProducesResponseType(typeof(DashboardDto), 200)]
        public async Task<ActionResult<DashboardDto>> GetDashboard()
        {
            // get today's date to filter "today's appointments"
            var today = DateTime.UtcNow.Date;

            var dashboard = new DashboardDto
            {
                // --- APPOINTMENT STATS ---
                TotalAppointments = await _context.Appointments.CountAsync(),
                ScheduledAppointments = await _context.Appointments.CountAsync(a => a.Status == "Scheduled"),
                CompletedAppointments = await _context.Appointments.CountAsync(a => a.Status == "Completed"),
                CancelledAppointments = await _context.Appointments.CountAsync(a => a.Status == "Cancelled"),
                TodayAppointments = await _context.Appointments.CountAsync(a => a.StartTime.Date == today),

                // --- STAFF STATS ---
                TotalStaff = await _context.Staffs.CountAsync(),
                ActiveStaff = await _context.Staffs.CountAsync(s => s.IsActive),

                // --- USER STATS ---
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                PendingApprovals = await _context.Users.CountAsync(u => !u.IsApproved),

                // --- RECENT APPOINTMENTS ---
                // grab the 5 newest appointments for the "recent activity" widget on the dashboard
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Staff)                    // join staff table for names
                    .OrderByDescending(a => a.CreatedAtUtc)   // newest first
                    .Take(5)                                   // only the top 5
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
                    .ToListAsync()
            };

            return Ok(dashboard);  // 200 OK with everything the dashboard needs
        }
    }
}
