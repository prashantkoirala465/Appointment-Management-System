using System;
using System.Linq;
using System.Threading.Tasks;
using AppointmentSystem.Web.Controllers;
using AppointmentSystem.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Data
{
    /// Seeds the database with the superadmin account and default system data
    /// Only the admin is seeded — staff members register through the signup page
    /// and must be approved by the admin before they can log in
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // --- Step 1: Seed Roles ---
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
            if (!await context.Menus.AnyAsync())
            {
                var menus = new[]
                {
                    new Menu { Id = Guid.NewGuid(), MenuName = "Appointments", Url = "/Appointments", DisplayOrder = 1, IsActive = true },
                    new Menu { Id = Guid.NewGuid(), MenuName = "Staff", Url = "/Staffs", DisplayOrder = 2, IsActive = true },
                    new Menu { Id = Guid.NewGuid(), MenuName = "Users", Url = "/Users", DisplayOrder = 3, IsActive = true },
                    new Menu { Id = Guid.NewGuid(), MenuName = "Roles", Url = "/Roles", DisplayOrder = 4, IsActive = true },
                    new Menu { Id = Guid.NewGuid(), MenuName = "Menus", Url = "/Menus", DisplayOrder = 5, IsActive = true },
                };

                context.Menus.AddRange(menus);
                await context.SaveChangesAsync();
            }

            // --- Step 3: Seed Superadmin ---
            // Only one admin is seeded. Staff accounts are created via registration
            if (!await context.Users.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.RoleName == "Admin");
                var allMenus = await context.Menus.ToListAsync();

                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "System Administrator",
                    Username = "admin",
                    Email = "admin@appointra.com",
                    PasswordHash = AccountController.HashPassword("admin123"),
                    IsActive = true,
                    IsApproved = true, // Superadmin is automatically approved
                    CreatedAtUtc = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                // Assign Admin role
                context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                await context.SaveChangesAsync();

                // Assign all menus to admin
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
