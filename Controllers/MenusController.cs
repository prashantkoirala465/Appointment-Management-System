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
    /// Admin controller for managing navigation menu items
    /// Handles CRUD operations for menu entries that get assigned to users
    [Authorize(Roles = "Admin")]
    public class MenusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Menus
        public async Task<IActionResult> Index()
        {
            var menus = await _context.Menus
                .Include(m => m.UserMenus)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();

            return View(menus);
        }

        // GET: /Menus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Menus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MenuName,Url,DisplayOrder,IsActive")] Menu menu)
        {
            if (!ModelState.IsValid) return View(menu);

            menu.Id = Guid.NewGuid();
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Menus/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound();

            return View(menu);
        }

        // POST: /Menus/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,MenuName,Url,DisplayOrder,IsActive")] Menu menu)
        {
            if (id != menu.Id) return NotFound();

            if (!ModelState.IsValid) return View(menu);

            try
            {
                _context.Update(menu);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Menus.AnyAsync(m => m.Id == menu.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Menus/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var menu = await _context.Menus
                .Include(m => m.UserMenus)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null) return NotFound();

            return View(menu);
        }

        // POST: /Menus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var menu = await _context.Menus
                .Include(m => m.UserMenus)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu != null)
            {
                // Remove all user-menu assignments first, then the menu itself
                _context.UserMenus.RemoveRange(menu.UserMenus);
                _context.Menus.Remove(menu);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
