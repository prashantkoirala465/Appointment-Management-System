using System;
using System.Linq;
using System.Threading.Tasks;
using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Controllers
{

    /// This controller manages everything related to appointments
    /// It's the heart of our appointment booking system
    /// Users can view, create, edit, and cancel appointments through this controller
    /// [Authorize] ensures only logged-in users can access these pages
    [Authorize]
    public class AppointmentsController : Controller
    {
        // Our connection to the database
        // We use this to query and save appointment data
        private readonly ApplicationDbContext _context;

        // Constructor: ASP.NET Core injects the database context automatically
        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Appointments
        // Shows a list of all appointments
        // We load the associated staff member for each appointment so we can display their name
        public async Task<IActionResult> Index()
        {
            // Fetch all appointments from the database
            // Include(a => a.Staff) loads the staff info too (prevents additional queries later)
            // OrderByDescending shows newest appointments first (most recent at the top)
            var appointments = await _context.Appointments
                .Include(a => a.Staff)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            // Send the appointments list to the view for display
            return View(appointments);
        }

        // GET: /Appointments/Details/5
        // Shows detailed information about a single appointment
        public async Task<IActionResult> Details(Guid? id)
        {
            // No ID provided? Can't show details without knowing which appointment
            if (id == null) return NotFound();

            // Load the appointment with its associated staff member
            var appointment = await _context.Appointments
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Appointment doesn't exist? Show 404 page
            if (appointment == null) return NotFound();

            // Display the details page with all the appointment info
            return View(appointment);
        }

        // GET: /Appointments/Create
        // Shows the form to book a new appointment
        public IActionResult Create()
        {
            // Populate the dropdown list with all available staff members
            // "Id" is the value that gets saved to the database
            // "FullName" is what users see in the dropdown
            ViewData["StaffId"] = new SelectList(_context.Staffs, "Id", "FullName");
            
            // Show the empty form
            return View();
        }

        // POST: /Appointments/Create
        // Processes the form submission when booking a new appointment
        [HttpPost]
        [ValidateAntiForgeryToken] // Security: prevents cross-site request forgery attacks
        public async Task<IActionResult> Create(
            [Bind("Id,StaffId,ClientName,ClientEmail,ClientPhone,StartTime,DurationMinutes,Status,Notes")]
            Appointment appointment)
        {
            // Check if all the form data is valid
            // This validates required fields, email format, phone format, etc.
            if (!ModelState.IsValid)
            {
                // Validation failed - reload the staff dropdown and show errors
                // The last parameter pre-selects the staff member the user chose
                ViewData["StaffId"] = new SelectList(_context.Staffs, "Id", "FullName", appointment.StaffId);
                return View(appointment);
            }

            // Generate a new unique ID for this appointment
            // GUIDs are globally unique, so no chance of conflicts
            appointment.Id = Guid.NewGuid();
            
            // Stamp it with the current time (in UTC to avoid timezone issues)
            appointment.CreatedAtUtc = DateTime.UtcNow;

            // Add the appointment to the database context
            _context.Add(appointment);
            
            // Actually save it to the database
            await _context.SaveChangesAsync();
            
            // Success! Redirect to the appointments list so user can see their new booking
            return RedirectToAction(nameof(Index));
        }

        // GET: /Appointments/Edit/5
        // Shows a form to edit an existing appointment
        public async Task<IActionResult> Edit(Guid? id)
        {
            // Need an ID to know which appointment to edit
            if (id == null) return NotFound();

            // Load the appointment from the database
            var appointment = await _context.Appointments.FindAsync(id);
            
            // Appointment not found? Show 404
            if (appointment == null) return NotFound();

            // Populate the staff dropdown, with the current staff member pre-selected
            // Using FullName to display staff names in the dropdown
            ViewData["StaffId"] = new SelectList(_context.Staffs, "Id", "FullName", appointment.StaffId);
            
            // Show the edit form with current data pre-filled
            return View(appointment);
        }

        // POST: /Appointments/Edit/5
        // Processes the form submission when updating an appointment
        [HttpPost]
        [ValidateAntiForgeryToken] // Security: prevents CSRF attacks
        public async Task<IActionResult> Edit(
            Guid id,
            [Bind("Id,StaffId,ClientName,ClientEmail,ClientPhone,StartTime,DurationMinutes,Status,Notes,CreatedAtUtc")]
            Appointment appointment)
        {
            // Security check: make sure the ID in the URL matches the appointment being edited
            if (id != appointment.Id) return NotFound();

            // Validate all the form data
            if (!ModelState.IsValid)
            {
                // Validation failed - reload dropdown and show errors
                // Pre-select the staff member the user chose before the error
                ViewData["StaffId"] = new SelectList(_context.Staffs, "Id", "FullName", appointment.StaffId);
                return View(appointment);
            }

            try
            {
                // Stamp the appointment with the current update time
                // This creates an audit trail of when changes were made
                appointment.UpdatedAtUtc = DateTime.UtcNow;

                // Tell EF Core this entity has been modified
                _context.Update(appointment);
                
                // Save the changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // This happens if someone else deleted the appointment while we were editing it
                // Check if the appointment still exists
                if (!AppointmentExists(appointment.Id)) return NotFound();
                
                // Different concurrency issue - let it bubble up to the error handler
                throw;
            }

            // Success! Go back to the appointments list
            return RedirectToAction(nameof(Index));
        }

        // GET: /Appointments/Delete/5
        // Shows a confirmation page before deleting an appointment
        // We don't delete immediately - we ask "Are you sure?" first
        public async Task<IActionResult> Delete(Guid? id)
        {
            // Need an ID to know what to delete
            if (id == null) return NotFound();

            // Load the appointment with staff info to show on the confirmation page
            var appointment = await _context.Appointments
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Appointment doesn't exist? Show 404
            if (appointment == null) return NotFound();

            // Show the delete confirmation page with appointment details
            return View(appointment);
        }

        // POST: /Appointments/Delete/5
        // Actually deletes the appointment after user confirms
        [HttpPost, ActionName("Delete")] // Maps to the Delete action even though method name is different
        [ValidateAntiForgeryToken] // Security: prevents CSRF attacks
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // Try to find the appointment
            var appointment = await _context.Appointments.FindAsync(id);
            
            if (appointment != null)
            {
                // Found it - remove it from the database
                _context.Appointments.Remove(appointment);
            }

            // Save the changes (this actually deletes it)
            await _context.SaveChangesAsync();
            
            // Return to the appointments list
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if an appointment exists
        // Used internally to verify records exist before operating on them
        private bool AppointmentExists(Guid id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
