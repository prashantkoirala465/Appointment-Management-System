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
    /// Admin-only controller for managing the navigation menu items
    /// Menu items are what show up in the sidebar ("Appointments", "Staff", etc.)
    /// Admins can create, edit, reorder, and delete menu entries
    /// Each menu is then assigned to specific users through the UserMenus junction table
    [Authorize(Roles = "Admin")] // only admin users can manage menus
    public class MenusController : Controller
    {
        // our database connection, injected automatically by the DI container
        private readonly ApplicationDbContext _context;

        // constructor receives the db context through dependency injection
        public MenusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Menus
        // shows all menu items in a table, sorted by their display order
        public async Task<IActionResult> Index()
        {
            // grab all menus from the database
            // Include(m => m.UserMenus) loads user assignments so we can show
            // how many users have each menu item assigned
            // OrderBy(m => m.DisplayOrder) sorts them the same way they appear in the sidebar
            var menus = await _context.Menus
                .Include(m => m.UserMenus)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();

            // hand off the list to the view for rendering
            return View(menus);
        }

        // GET: /Menus/Create
        // shows a blank form for creating a new sidebar menu entry
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Menus/Create
        // processes the form and saves the new menu item to the database
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF protection — verify the form came from our site
        public async Task<IActionResult> Create([Bind("MenuName,Url,DisplayOrder,IsActive")] Menu menu)
        {
            // if something didn't pass validation (empty name, etc.), show the form again
            if (!ModelState.IsValid) return View(menu);

            // generate a unique GUID for this menu item's primary key
            menu.Id = Guid.NewGuid();
            // queue it for insertion
            _context.Menus.Add(menu);
            // write to the SQLite database
            await _context.SaveChangesAsync();

            // done — redirect back to the list so admin sees their new menu
            return RedirectToAction(nameof(Index));
        }

        // GET: /Menus/Edit/5
        // loads a menu item and shows the edit form with current values pre-filled
        public async Task<IActionResult> Edit(Guid? id)
        {
            // no id in the URL means we don't know what to edit
            if (id == null) return NotFound();

            // look up the menu by its primary key
            var menu = await _context.Menus.FindAsync(id);
            // doesn't exist? show 404
            if (menu == null) return NotFound();

            // render the edit form with the current data already filled in
            return View(menu);
        }

        // POST: /Menus/Edit/5
        // saves the updated menu item back to the database
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF token check on every form POST
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,MenuName,Url,DisplayOrder,IsActive")] Menu menu)
        {
            // make sure the URL id matches the form data — prevents tampering
            if (id != menu.Id) return NotFound();

            // re-check all validation rules
            if (!ModelState.IsValid) return View(menu);

            try
            {
                // mark this entity as modified so EF Core generates an UPDATE statement
                _context.Update(menu);
                // execute the update against the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // someone else might have deleted this menu while we were editing
                // check if it still exists
                if (!await _context.Menus.AnyAsync(m => m.Id == menu.Id)) return NotFound();
                // if it does exist, something else went wrong — rethrow
                throw;
            }

            // update succeeded — go back to the list
            return RedirectToAction(nameof(Index));
        }

        // GET: /Menus/Delete/5
        // shows a confirmation page before we actually delete
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            // load menu with its user assignments so the view can warn the admin
            // about how many users will lose this menu item
            var menu = await _context.Menus
                .Include(m => m.UserMenus)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null) return NotFound();

            // render the "are you sure?" page
            return View(menu);
        }

        // POST: /Menus/Delete/5
        // permanently removes the menu item and all its user assignments
        [HttpPost, ActionName("Delete")] // maps this method to the Delete action URL
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // load the menu along with which users have it assigned
            var menu = await _context.Menus
                .Include(m => m.UserMenus)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu != null)
            {
                // first wipe out all the user-menu links (foreign key constraint)
                // if we skipped this, the database would reject the delete
                _context.UserMenus.RemoveRange(menu.UserMenus);
                // now delete the menu item itself
                _context.Menus.Remove(menu);
                // save both operations in one go
                await _context.SaveChangesAsync();
            }

            // back to the menus list
            return RedirectToAction(nameof(Index));
        }
    }
}
