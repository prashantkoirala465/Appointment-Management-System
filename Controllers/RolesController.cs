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
    /// Admin-only controller for managing roles in the system
    /// Roles (like "Admin", "Staff") determine what pages a user can access
    /// Only admin users can create, edit, or delete roles
    [Authorize(Roles = "Admin")] // restricts the entire controller to admin users
    public class RolesController : Controller
    {
        // database context — our connection to the database, injected by DI
        private readonly ApplicationDbContext _context;

        // constructor — ASP.NET Core automatically passes in the database context
        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Roles
        // shows all roles in a list — we also pull in how many users have each role
        public async Task<IActionResult> Index()
        {
            // fetch every role from the database
            // Include(r => r.UserRoles) also loads the user-role links so we can count assigned users
            // OrderBy sorts alphabetically so Admin comes before Staff
            var roles = await _context.Roles
                .Include(r => r.UserRoles)
                .OrderBy(r => r.RoleName)
                .ToListAsync();

            // pass the list to the view which renders the HTML table
            return View(roles);
        }

        // GET: /Roles/Create
        // just shows the empty form — no database work needed here
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Roles/Create
        // handles the form submission when admin creates a new role
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF protection — makes sure the form came from our site
        public async Task<IActionResult> Create([Bind("RoleName,Description,IsActive")] Role role)
        {
            // if form validation failed (e.g. empty role name), show the form again with errors
            if (!ModelState.IsValid) return View(role);

            // make sure no other role already has this name — role names must be unique
            if (await _context.Roles.AnyAsync(r => r.RoleName == role.RoleName))
            {
                ModelState.AddModelError("RoleName", "A role with this name already exists.");
                return View(role);
            }

            // generate a new GUID as the primary key for this role
            role.Id = Guid.NewGuid();
            // add the role to the context (queued for saving)
            _context.Roles.Add(role);
            // actually write it to the database
            await _context.SaveChangesAsync();

            // all good — send the admin back to the roles list
            return RedirectToAction(nameof(Index));
        }

        // GET: /Roles/Edit/5
        // loads the role's current data and shows the edit form
        public async Task<IActionResult> Edit(Guid? id)
        {
            // can't edit without knowing which role
            if (id == null) return NotFound();

            // FindAsync searches by primary key — fast and simple
            var role = await _context.Roles.FindAsync(id);
            // if the id doesn't match any role, show 404
            if (role == null) return NotFound();

            // show the edit form pre-filled with this role's data
            return View(role);
        }

        // POST: /Roles/Edit/5
        // saves the updated role back to the database
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF protection on every POST
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,RoleName,Description,IsActive")] Role role)
        {
            // safety check: URL id must match the form's role id
            if (id != role.Id) return NotFound();

            // if validation rules failed, redisplay the form with error messages
            if (!ModelState.IsValid) return View(role);

            // check for duplicate name, but exclude THIS role from the check
            // (so renaming "Admin" to "Admin" doesn't trigger a false duplicate)
            if (await _context.Roles.AnyAsync(r => r.RoleName == role.RoleName && r.Id != id))
            {
                ModelState.AddModelError("RoleName", "A role with this name already exists.");
                return View(role);
            }

            try
            {
                // tell EF Core this entity has been modified and needs to be saved
                _context.Update(role);
                // write changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // this happens when two people edit the same role at the same time
                // check if the role still exists — if not, show 404
                if (!await _context.Roles.AnyAsync(r => r.Id == role.Id)) return NotFound();
                // otherwise it's a real conflict — rethrow so the error page can handle it
                throw;
            }

            // success — redirect back to the roles list
            return RedirectToAction(nameof(Index));
        }

        // GET: /Roles/Delete/5
        // shows a confirmation page — "are you sure you want to delete this role?"
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            // load role with its user assignments so we can warn the admin
            // how many users currently have this role
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null) return NotFound();

            // show the confirmation page with the role details
            return View(role);
        }

        // POST: /Roles/Delete/5
        // actually deletes the role after admin confirms
        [HttpPost, ActionName("Delete")] // ActionName maps this to the "Delete" route
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // load the role along with all its user-role assignments
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role != null)
            {
                // we have to remove user-role assignments first because of the foreign key constraint
                // if we tried to delete the role directly, the database would throw an error
                _context.UserRoles.RemoveRange(role.UserRoles);
                // now we can safely remove the role itself
                _context.Roles.Remove(role);
                // commit both deletions in a single transaction
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
