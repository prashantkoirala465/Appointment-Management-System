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
    /// This is one of the most complex controllers because it handles:
    ///   1. CRUD for user accounts
    ///   2. Role assignments (which roles a user has)
    ///   3. Menu assignments (which sidebar items a user sees)
    ///   4. Approve/Reject workflow for staff registrations
    [Authorize(Roles = "Admin")] // only admins can manage users
    public class UsersController : Controller
    {
        // database context injected through dependency injection
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Users
        // Lists all users with their assigned roles in a table
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)       // eager-load the user-role junction entries
                    .ThenInclude(ur => ur.Role)   // also load the actual Role entity for each
                .OrderByDescending(u => !u.IsApproved) // trick: unapproved (pending) users float to top
                .ThenBy(u => u.FullName)          // within each group, sort alphabetically
                .ToListAsync();

            return View(users);
        }

        // POST: /Users/Approve/5
        // When a staff member registers, their account starts as unapproved (IsApproved = false)
        // An admin clicks "Approve" to flip that flag so the staff can log in
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF protection
        public async Task<IActionResult> Approve(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // just flip the flag — that's all it takes to let them log in
            user.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/Reject/5
        // Admin doesn't want this staff member — delete their registration entirely
        // We also have to remove their role and menu assignments because of foreign key constraints
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id)
        {
            // load the user along with their role and menu assignments
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserMenus)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            // clean up junction table records first (FK constraint requires this)
            _context.UserRoles.RemoveRange(user.UserRoles);
            _context.UserMenus.RemoveRange(user.UserMenus);
            // now we can safely delete the user
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
        // Processes the form, creates the user, then assigns whatever roles and menus the admin checked
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF protection
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            // manual validation: password is required for new accounts
            // (on edit, leaving password blank means "don't change it")
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new users.");
            }

            // if validation failed, re-populate the checkboxes and show the form again
            if (!ModelState.IsValid)
            {
                await PopulateAssignments(model);
                return View(model);
            }

            // make sure no one else already has this username
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                await PopulateAssignments(model); // reload checkboxes again
                return View(model);
            }

            // build the new user entity
            var user = new User
            {
                Id = Guid.NewGuid(),                // generate a unique ID
                FullName = model.FullName,
                Username = model.Username,
                Email = model.Email,
                PasswordHash = AccountController.HashPassword(model.Password!), // SHA-256 hash
                IsActive = model.IsActive,
                IsApproved = true,                   // admin-created users skip the approval step
                CreatedAtUtc = DateTime.UtcNow       // track when the account was created
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // save so the user gets an ID in the database

            // now create junction records for every role checkbox that was ticked
            foreach (var role in model.Roles.Where(r => r.IsSelected))
            {
                _context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = role.RoleId
                });
            }

            // same for menu checkboxes — determines which sidebar items this user sees
            foreach (var menu in model.Menus.Where(m => m.IsSelected))
            {
                _context.UserMenus.Add(new UserMenu
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    MenuId = menu.MenuId
                });
            }

            await _context.SaveChangesAsync(); // save roles + menus in one batch
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
        // This is the most complex action — updates user info AND syncs role/menu assignments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UserFormViewModel model)
        {
            // safety check: URL id must match the form's hidden id field
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateAssignments(model); // re-load checkboxes so the form still works
                return View(model);
            }

            // load the user along with their current role and menu assignments
            // we need these loaded so we can delete the old ones before saving new ones
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserMenus)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            // prevent duplicate usernames, but exclude this user's own current username
            if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.Id != id))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                await PopulateAssignments(model);
                return View(model);
            }

            // update the basic text fields from the form
            user.FullName = model.FullName;
            user.Username = model.Username;
            user.Email = model.Email;
            user.IsActive = model.IsActive;

            // only update password if admin typed a new one
            // blank = leave the existing password unchanged
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = AccountController.HashPassword(model.Password);
            }

            // --- ROLE SYNC ---
            // strategy: delete ALL existing role assignments, then re-create from the checked boxes
            // this "delete-all-then-insert" approach is simpler than diffing
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

            // --- MENU SYNC ---
            // same pattern: wipe existing menu assignments, re-create from checkboxes
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

            // save everything — user fields + role changes + menu changes — in one transaction
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
        // permanently deletes the user account and cleans up all junction table records
        [HttpPost, ActionName("Delete")] // maps this method to the Delete action URL
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // load user with all their assignments so we can clean them up
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserMenus)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user != null)
            {
                // must delete junction records first — FK constraint would block the user delete
                _context.UserRoles.RemoveRange(user.UserRoles);
                _context.UserMenus.RemoveRange(user.UserMenus);
                // now safely delete the user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /// Helper method: loads all active roles and menus into the view model as checkbox lists
        /// Used by both Create and Edit actions so the admin can tick which roles/menus to assign
        /// If existingRoles/existingMenus are provided (Edit mode), those checkboxes come pre-ticked
        private async Task PopulateAssignments(
            UserFormViewModel model,
            ICollection<UserRole>? existingRoles = null,
            ICollection<UserMenu>? existingMenus = null)
        {
            // grab all active roles and menus from the database
            var allRoles = await _context.Roles.Where(r => r.IsActive).OrderBy(r => r.RoleName).ToListAsync();
            var allMenus = await _context.Menus.Where(m => m.IsActive).OrderBy(m => m.DisplayOrder).ToListAsync();

            // build a HashSet of IDs for O(1) lookup when marking checkboxes
            var assignedRoleIds = existingRoles?.Select(r => r.RoleId).ToHashSet() ?? new HashSet<Guid>();
            var assignedMenuIds = existingMenus?.Select(m => m.MenuId).ToHashSet() ?? new HashSet<Guid>();

            // project each role into a RoleAssignment DTO with IsSelected = true if already assigned
            model.Roles = allRoles.Select(r => new RoleAssignment
            {
                RoleId = r.Id,
                RoleName = r.RoleName,
                IsSelected = assignedRoleIds.Contains(r.Id) // pre-tick if user already has this role
            }).ToList();

            // same for menus
            model.Menus = allMenus.Select(m => new MenuAssignment
            {
                MenuId = m.Id,
                MenuName = m.MenuName,
                IsSelected = assignedMenuIds.Contains(m.Id) // pre-tick if user already has this menu
            }).ToList();
        }
    }
}
