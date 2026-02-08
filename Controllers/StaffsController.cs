using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Models;

namespace AppointmentSystem.Web.Controllers
{
    /// This controller manages all staff-related operations
    /// It handles viewing, creating, editing, and deleting staff members
    /// Think of it as the "Staff Management" section of our app
    public class StaffsController : Controller
    {
        // Our database context - this is how we talk to the database
        // The underscore prefix is a C# convention for private fields
        private readonly ApplicationDbContext _context;

        // Constructor: ASP.NET Core automatically injects the database context
        // This happens through dependency injection configured in Program.cs
        public StaffsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Staffs
        // This shows a list of all staff members
        // We sort them alphabetically by name to make them easier to find
        public async Task<IActionResult> Index()
        {
            // Fetch all staff from database and sort them by name
            // ToListAsync() executes the query asynchronously so we don't block the thread
            var staff = await _context.Staffs
                .OrderBy(s => s.FullName)
                .ToListAsync();

            // Pass the list of staff to the view to display
            return View(staff);
        }

        // GET: /Staffs/Details/5
        // Shows detailed information about a specific staff member
        // The id parameter comes from the URL (e.g., /Staffs/Details/abc123)
        public async Task<IActionResult> Details(Guid? id)
        {
            // If no ID was provided in the URL, return 404 Not Found
            // The ? after Guid makes it nullable, so we can detect missing IDs
            if (id == null)
            {
                return NotFound();
            }

            // Load the staff member from the database
            // Include() loads their appointments too (eager loading)
            // This saves us from making separate database calls later
            var staff = await _context.Staffs
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(m => m.Id == id);

            // If we couldn't find a staff member with that ID, return 404
            if (staff == null)
            {
                return NotFound();
            }

            // Show the details page with this staff member's info
            return View(staff);
        }

        // GET: /Staffs/Create
        // Shows the form to create a new staff member
        // This just displays an empty form - no database work yet
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Staffs/Create
        // This handles the form submission when creating a new staff member
        // [HttpPost] means this only responds to POST requests (form submissions)
        [HttpPost]
        [ValidateAntiForgeryToken] // Security: prevents CSRF attacks
        public async Task<IActionResult> Create([Bind("Id,FullName,Email,PhoneNumber,Specialty,IsActive")] Staff staff)
        {
            // ModelState.IsValid checks if all validation rules passed
            // (e.g., required fields filled, email format correct, etc.)
            if (ModelState.IsValid)
            {
                // Add the new staff member to the database context
                _context.Add(staff);
                
                // SaveChangesAsync() actually writes to the database
                // It's async so we don't block the thread while waiting for the DB
                await _context.SaveChangesAsync();
                
                // Redirect to the list page so the user sees their new staff member
                return RedirectToAction(nameof(Index));
            }

            // If validation failed, show the form again with error messages
            // The staff object still has the user's input, so they don't have to retype everything
            return View(staff);
        }

        // GET: /Staffs/Edit/5
        // Shows a form to edit an existing staff member
        public async Task<IActionResult> Edit(Guid? id)
        {
            // Null check: can't edit a staff member without knowing which one
            if (id == null)
            {
                return NotFound();
            }

            // Find the staff member by ID
            // FindAsync is optimized for searching by primary key
            var staff = await _context.Staffs.FindAsync(id);
            
            // If the ID doesn't exist in our database, show 404
            if (staff == null)
            {
                return NotFound();
            }

            // Show the edit form pre-filled with this staff member's current data
            return View(staff);
        }

        // POST: /Staffs/Edit/5
        // Handles the form submission when updating a staff member
        [HttpPost]
        [ValidateAntiForgeryToken] // Security: prevents CSRF attacks
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FullName,Email,PhoneNumber,Specialty,IsActive")] Staff staff)
        {
            // Security check: make sure the ID in the URL matches the ID in the form
            // This prevents someone from editing a different staff member than intended
            if (id != staff.Id)
            {
                return NotFound();
            }

            // Check if all validation rules passed
            if (ModelState.IsValid)
            {
                try
                {
                    // Mark this entity as modified so EF Core knows to update it
                    _context.Update(staff);
                    
                    // Save the changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // This happens if someone else deleted this record while we were editing
                    // Check if the staff member still exists
                    if (!StaffExists(staff.Id))
                    {
                        return NotFound();
                    }
                    
                    // If it's a different concurrency issue, rethrow the exception
                    // The error handler will catch it and show an error page
                    throw;
                }

                // Success! Go back to the list page
                return RedirectToAction(nameof(Index));
            }

            // Validation failed - show the form again with error messages
            return View(staff);
        }

        // GET: /Staffs/Delete/5
        // Shows a confirmation page before deleting a staff member
        // We don't delete immediately - we ask "Are you sure?" first
        public async Task<IActionResult> Delete(Guid? id)
        {
            // Can't delete without knowing which staff member
            if (id == null)
            {
                return NotFound();
            }

            // Load the staff member to show their info on the confirmation page
            var staff = await _context.Staffs.FirstOrDefaultAsync(m => m.Id == id);
            
            // If they don't exist, show 404
            if (staff == null)
            {
                return NotFound();
            }

            // Show the delete confirmation page with this staff member's details
            return View(staff);
        }

        // POST: /Staffs/Delete/5
        // Actually performs the deletion after user confirms
        [HttpPost, ActionName("Delete")] // ActionName("Delete") makes this respond to the Delete action
        [ValidateAntiForgeryToken] // Security: prevents CSRF attacks
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // Load the staff member along with their appointments
            // We need the appointments to make a smart deletion decision
            var staff = await _context.Staffs
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff != null)
            {
                // Smart deletion: If this staff member has appointments, don't hard delete
                // Instead, just mark them as inactive to preserve historical data
                // This is called a "soft delete" - the record stays in the database
                if (staff.Appointments.Any())
                {
                    // They have appointments, so just deactivate them
                    staff.IsActive = false;
                    _context.Staffs.Update(staff);
                }
                else
                {
                    // No appointments, safe to permanently delete
                    _context.Staffs.Remove(staff);
                }
            }

            // Save the changes (either the soft delete or hard delete)
            await _context.SaveChangesAsync();
            
            // Return to the staff list page
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a staff member exists in the database
        // Used internally by other methods to verify records exist
        // Returns true if found, false if not
        private bool StaffExists(Guid id)
        {
            return _context.Staffs.Any(e => e.Id == id);
        }
    }
}
