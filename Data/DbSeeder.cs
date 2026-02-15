using System;
using System.Linq;
using System.Threading.Tasks;
using AppointmentSystem.Web.Controllers;
using AppointmentSystem.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Data
{
    /// This class seeds the database with initial data when the application starts
    /// It creates default roles, users, menus, and their assignments
    /// This ensures the system has at least one admin user so someone can log in
    /// The seed only runs if the data doesn't already exist (safe to run multiple times)
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // --- Step 1: Seed Roles ---
            // Create the default roles if they don't exist yet
            if (!await context.Roles.AnyAsync())
            {
                var adminRole = new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "Admin",
                    Description = "Full access to all features including user and staff management",
                    IsActive = true
                };

                var staffRole = new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "Staff",
                    Description = "Can view and manage appointments",
                    IsActive = true
                };

                context.Roles.AddRange(adminRole, staffRole);
                await context.SaveChangesAsync();
            }

            // --- Step 2: Seed Menus ---
            // Create the default navigation menu items if they don't exist yet
            if (!await context.Menus.AnyAsync())
            {
                var menus = new[]
                {
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        MenuName = "Appointments",
                        Url = "/Appointments",
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        MenuName = "Staff",
                        Url = "/Staffs",
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        MenuName = "Privacy",
                        Url = "/Home/Privacy",
                        DisplayOrder = 3,
                        IsActive = true
                    }
                };

                context.Menus.AddRange(menus);
                await context.SaveChangesAsync();
            }

            // --- Step 3: Seed Default Users ---
            // Create the default admin and staff users if no users exist yet
            if (!await context.Users.AnyAsync())
            {
                // Look up the roles we just created
                var adminRole = await context.Roles.FirstAsync(r => r.RoleName == "Admin");
                var staffRole = await context.Roles.FirstAsync(r => r.RoleName == "Staff");

                // Get all menus for assigning to users
                var allMenus = await context.Menus.ToListAsync();

                // Create the admin user with password "admin123"
                // The password is hashed before storage - never stored in plain text!
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "System Administrator",
                    Username = "admin",
                    Email = "admin@appointment.com",
                    PasswordHash = AccountController.HashPassword("admin123"),
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                // Create a staff user with password "staff123"
                var staffUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "John Staff",
                    Username = "staff",
                    Email = "staff@appointment.com",
                    PasswordHash = AccountController.HashPassword("staff123"),
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                context.Users.AddRange(adminUser, staffUser);
                await context.SaveChangesAsync();

                // --- Step 4: Assign Roles to Users ---
                // Admin user gets the Admin role
                context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });

                // Staff user gets the Staff role
                context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = staffUser.Id,
                    RoleId = staffRole.Id
                });

                await context.SaveChangesAsync();

                // --- Step 5: Assign Menus to Users ---
                // Admin user gets ALL menus (full access to navigation)
                foreach (var menu in allMenus)
                {
                    context.UserMenus.Add(new UserMenu
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminUser.Id,
                        MenuId = menu.Id
                    });
                }

                // Staff user only gets the Appointments menu
                // They shouldn't see Staff management or other admin-level menus
                var appointmentsMenu = allMenus.First(m => m.MenuName == "Appointments");
                context.UserMenus.Add(new UserMenu
                {
                    Id = Guid.NewGuid(),
                    UserId = staffUser.Id,
                    MenuId = appointmentsMenu.Id
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
