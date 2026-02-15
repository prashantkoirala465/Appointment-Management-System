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
    /// Admin controller for managing user accounts
    /// Handles CRUD operations plus role and menu assignments for each user
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Users
        // Lists all users with their assigned roles
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => !u.IsApproved) // Pending users first
                .ThenBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }

        // POST: /Users/Approve/5
        // Approves a pending staff registration so they can log in
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/Reject/5
        // Rejects (deletes) a pending staff registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserMenus)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            _context.UserRoles.RemoveRange(user.UserRoles);
            _context.UserMenus.RemoveRange(user.UserMenus);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Users/Create
        // Shows the form for creating a new user with role and menu checkboxes
        public async Task<IActionResult> Create()
        {
            var viewModel = new UserFormViewModel();
            await PopulateAssignments(viewModel);
            return View(viewModel);
        }

        // POST: /Users/Create
        // Creates a new user and assigns selected roles and menus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            // Password is required when creating a new user
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new users.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateAssignments(model);
                return View(model);
            }

            // Check for duplicate username
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                await PopulateAssignments(model);
                return View(model);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = model.FullName,
                Username = model.Username,
                Email = model.Email,
                PasswordHash = AccountController.HashPassword(model.Password!),
                IsActive = model.IsActive,
                IsApproved = true, // Admin-created users are automatically approved
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign selected roles
            foreach (var role in model.Roles.Where(r => r.IsSelected))
            {
                _context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = role.RoleId
                });
            }

            // Assign selected menus
            foreach (var menu in model.Menus.Where(m => m.IsSelected))
            {
                _context.UserMenus.Add(new UserMenu
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    MenuId = menu.MenuId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Users/Edit/5
        // Shows the edit form with current role and menu assignments pre-checked
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserMenus)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var viewModel = new UserFormViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive
            };

            await PopulateAssignments(viewModel, user.UserRoles, user.UserMenus);
            return View(viewModel);
        }

        // POST: /Users/Edit/5
        // Updates the user and syncs role and menu assignments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UserFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateAssignments(model);
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserMenus)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            // Check for duplicate username (excluding current user)
            if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.Id != id))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                await PopulateAssignments(model);
                return View(model);
            }

            // Update basic fields
            user.FullName = model.FullName;
            user.Username = model.Username;
            user.Email = model.Email;
            user.IsActive = model.IsActive;

            // Update password only if a new one was provided
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = AccountController.HashPassword(model.Password);
            }

            // Sync roles: remove old, add new
            _context.UserRoles.RemoveRange(user.UserRoles);
            foreach (var role in model.Roles.Where(r => r.IsSelected))
            {
                _context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = role.RoleId
                });
            }

            // Sync menus: remove old, add new
            _context.UserMenus.RemoveRange(user.UserMenus);
            foreach (var menu in model.Menus.Where(m => m.IsSelected))
            {
                _context.UserMenus.Add(new UserMenu
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    MenuId = menu.MenuId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Users/Delete/5
        // Shows delete confirmation with user details
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /Users/Delete/5
        // Permanently deletes the user and their role/menu assignments
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserMenus)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user != null)
            {
                _context.UserRoles.RemoveRange(user.UserRoles);
                _context.UserMenus.RemoveRange(user.UserMenus);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Loads all available roles and menus into the view model
        // If existing assignments are provided, marks those as selected
        private async Task PopulateAssignments(
            UserFormViewModel model,
            ICollection<UserRole>? existingRoles = null,
            ICollection<UserMenu>? existingMenus = null)
        {
            var allRoles = await _context.Roles.Where(r => r.IsActive).OrderBy(r => r.RoleName).ToListAsync();
            var allMenus = await _context.Menus.Where(m => m.IsActive).OrderBy(m => m.DisplayOrder).ToListAsync();

            var assignedRoleIds = existingRoles?.Select(r => r.RoleId).ToHashSet() ?? new HashSet<Guid>();
            var assignedMenuIds = existingMenus?.Select(m => m.MenuId).ToHashSet() ?? new HashSet<Guid>();

            model.Roles = allRoles.Select(r => new RoleAssignment
            {
                RoleId = r.Id,
                RoleName = r.RoleName,
                IsSelected = assignedRoleIds.Contains(r.Id)
            }).ToList();

            model.Menus = allMenus.Select(m => new MenuAssignment
            {
                MenuId = m.Id,
                MenuName = m.MenuName,
                IsSelected = assignedMenuIds.Contains(m.Id)
            }).ToList();
        }
    }
}
