using System;
using System.Linq;
using System.Threading.Tasks;
using AppointmentSystem.Web.Controllers;
using AppointmentSystem.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Data
{
    /// Seeds the database with initial data on first run (Ananta's module)
    /// This runs once when the database is empty — creates:
    ///   1. Two default roles (Admin, Staff)
    ///   2. Five navigation menu items (Appointments, Staff, Users, Roles, Menus)
    ///   3. One superadmin account with all roles and all menus assigned
    /// Staff members are NOT seeded — they register via the signup page
    /// and wait for admin approval before they can log in
    public static class DbSeeder
    {
        /// Called from Program.cs during app startup
        /// Each section checks if data already exists before inserting
        /// This makes it safe to run multiple times (idempotent)
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // --- Step 1: Seed Roles ---
            // only seed if the Roles table is completely empty
            if (!await context.Roles.AnyAsync())
            {
                // Admin role — has full access to everything
                var adminRole = new Role
                {
                    Id = Guid.NewGuid(),             // generate unique ID
                    RoleName = "Admin",
                    Description = "Full access to all features including user and staff management",
                    IsActive = true
                };

                // Staff role — limited access, mainly appointment viewing
                var staffRole = new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "Staff",
                    Description = "Can view and manage appointments",
                    IsActive = true
                };

                // add both roles and save to the database
                context.Roles.AddRange(adminRole, staffRole);
                await context.SaveChangesAsync();
            }

            // --- Step 2: Seed Menus ---
            // create the default sidebar navigation items
            // DisplayOrder controls the sort position in the sidebar
            if (!await context.Menus.AnyAsync())
            {
                var menus = new[]
                {
                    new Menu { Id = Guid.NewGuid(), MenuName = "Appointments", Url = "/Appointments", DisplayOrder = 1, IsActive = true },
                    new Menu { Id = Guid.NewGuid(), MenuName = "Staff",        Url = "/Staffs",       DisplayOrder = 2, IsActive = true },
                    new Menu { Id = Guid.NewGuid(), MenuName = "Users",        Url = "/Users",        DisplayOrder = 3, IsActive = true },
                    new Menu { Id = Guid.NewGuid(), MenuName = "Roles",        Url = "/Roles",        DisplayOrder = 4, IsActive = true },
                    new Menu { Id = Guid.NewGuid(), MenuName = "Menus",        Url = "/Menus",        DisplayOrder = 5, IsActive = true },
                };

                context.Menus.AddRange(menus);
                await context.SaveChangesAsync();
            }

            // --- Step 3: Seed Superadmin Account ---
            // only one admin is seeded — this is the account you log in with initially
            // credentials: admin / admin123 (should be changed in production!)
            if (!await context.Users.AnyAsync())
            {
                // look up the Admin role we just created (need its ID for the junction table)
                var adminRole = await context.Roles.FirstAsync(r => r.RoleName == "Admin");
                // grab all menus so we can assign them all to the admin
                var allMenus = await context.Menus.ToListAsync();

                // create the superadmin user
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "System Administrator",
                    Username = "admin",
                    Email = "admin@appointra.com",
                    PasswordHash = AccountController.HashPassword("admin123"), // SHA-256 hash of the default password
                    IsActive = true,
                    IsApproved = true,       // superadmin skips the approval step
                    CreatedAtUtc = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();    // save so the user gets an ID in the database

                // link the admin user to the Admin role via the junction table
                context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                await context.SaveChangesAsync();

                // give the admin ALL sidebar menu items
                // (regular staff will only get specific menus assigned by the admin)
                foreach (var menu in allMenus)
                {
                    context.UserMenus.Add(new UserMenu
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminUser.Id,
                        MenuId = menu.Id
                    });
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
