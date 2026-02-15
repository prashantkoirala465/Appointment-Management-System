using System;
using System.Linq;
using System.Threading.Tasks;
using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Controllers
{
    /// This controller displays the dashboard - a summary overview of the entire system
    /// It shows key metrics like appointment counts, staff stats, and recent activity
    /// Only authenticated users can access the dashboard
    [Authorize]
    public class DashboardController : Controller
    {
        // Our database context for querying appointment, staff, and user data
        private readonly ApplicationDbContext _context;

        // Constructor: ASP.NET Core injects the database context automatically
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Dashboard
        // Gathers all the statistics and displays the dashboard view
        // This is the main overview page that gives users a bird's-eye view of the system
        public async Task<IActionResult> Index()
        {
            // Get today's date for filtering today's appointments
            var today = DateTime.UtcNow.Date;

            // Build the dashboard view model with all the statistics
            var viewModel = new DashboardViewModel
            {
                // --- Appointment Statistics ---
                // Count total appointments in the system
                TotalAppointments = await _context.Appointments.CountAsync(),

                // Count appointments by status
                // These help the user see how many are scheduled, done, or cancelled
                ScheduledAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == "Scheduled"),

                CompletedAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == "Completed"),

                CancelledAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == "Cancelled"),

                // Count appointments scheduled for today
                // Compares just the date part (ignoring time) to find today's bookings
                TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.StartTime.Date == today),

                // --- Staff Statistics ---
                TotalStaff = await _context.Staffs.CountAsync(),
                ActiveStaff = await _context.Staffs.CountAsync(s => s.IsActive),

                // --- User Statistics ---
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                PendingApprovals = await _context.Users.CountAsync(u => !u.IsApproved),

                // --- Recent Activity ---
                // Get the 5 most recent appointments with their staff member info
                // This gives a quick glance at what's happening lately
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Staff)
                    .OrderByDescending(a => a.CreatedAtUtc)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}
