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
    /// API controller for dashboard statistics
    /// Returns aggregated data for the dashboard overview
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/dashboardapi
        /// Returns all dashboard statistics including appointment counts,
        /// staff/user counts, pending approvals, and recent appointments
        [HttpGet]
        [ProducesResponseType(typeof(DashboardDto), 200)]
        public async Task<ActionResult<DashboardDto>> GetDashboard()
        {
            var today = DateTime.UtcNow.Date;

            var dashboard = new DashboardDto
            {
                TotalAppointments = await _context.Appointments.CountAsync(),
                ScheduledAppointments = await _context.Appointments.CountAsync(a => a.Status == "Scheduled"),
                CompletedAppointments = await _context.Appointments.CountAsync(a => a.Status == "Completed"),
                CancelledAppointments = await _context.Appointments.CountAsync(a => a.Status == "Cancelled"),
                TodayAppointments = await _context.Appointments.CountAsync(a => a.StartTime.Date == today),
                TotalStaff = await _context.Staffs.CountAsync(),
                ActiveStaff = await _context.Staffs.CountAsync(s => s.IsActive),
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                PendingApprovals = await _context.Users.CountAsync(u => !u.IsApproved),
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Staff)
                    .OrderByDescending(a => a.CreatedAtUtc)
                    .Take(5)
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

            return Ok(dashboard);
        }
    }
}
