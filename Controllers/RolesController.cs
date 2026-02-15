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
    /// Admin controller for managing roles
    /// Handles CRUD operations for roles like Admin, Staff, etc.
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles
                .Include(r => r.UserRoles)
                .OrderBy(r => r.RoleName)
                .ToListAsync();

            return View(roles);
        }

        // GET: /Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleName,Description,IsActive")] Role role)
        {
            if (!ModelState.IsValid) return View(role);

            if (await _context.Roles.AnyAsync(r => r.RoleName == role.RoleName))
            {
                ModelState.AddModelError("RoleName", "A role with this name already exists.");
                return View(role);
            }

            role.Id = Guid.NewGuid();
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Roles/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            return View(role);
        }

        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,RoleName,Description,IsActive")] Role role)
        {
            if (id != role.Id) return NotFound();

            if (!ModelState.IsValid) return View(role);

            // Check for duplicate name (excluding current)
            if (await _context.Roles.AnyAsync(r => r.RoleName == role.RoleName && r.Id != id))
            {
                ModelState.AddModelError("RoleName", "A role with this name already exists.");
                return View(role);
            }

            try
            {
                _context.Update(role);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Roles.AnyAsync(r => r.Id == role.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Roles/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null) return NotFound();

            return View(role);
        }

        // POST: /Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role != null)
            {
                // Remove all user-role assignments first, then the role itself
                _context.UserRoles.RemoveRange(role.UserRoles);
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
