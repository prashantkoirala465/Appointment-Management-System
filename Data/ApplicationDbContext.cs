using Microsoft.EntityFrameworkCore;
using AppointmentSystem.Web.Models;

namespace AppointmentSystem.Web.Data
{
    /// This is our database context - the main gateway between our app and the database
    /// Think of it as a session with the database where we can query and save data
    public class ApplicationDbContext : DbContext
    {
        // Constructor: This gets called automatically by ASP.NET Core's dependency injection
        // The options parameter tells EF Core which database to connect to and how
        // We configure these options in Program.cs (using SQLite in our case)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet is like a table in our database
        // Each DbSet<T> represents a collection of entities that we can query, add, update, or delete
        
        // This represents our Appointments table
        // We can do things like: _context.Appointments.Where(a => a.Status == "Scheduled")
        public DbSet<Appointment> Appointments { get; set; }
        
        // This represents our Staffs table
        // We can query it like: _context.Staffs.Where(s => s.IsActive)
        public DbSet<Staff> Staffs { get; set; }

        // This represents our Users table - stores all user accounts
        // Users can log in and are assigned roles and menus
        public DbSet<User> Users { get; set; }

        // This represents our Roles table - stores role definitions
        // Roles like "Admin", "Staff", "User" control access levels
        public DbSet<Role> Roles { get; set; }

        // This represents the UserRoles junction table
        // It links users to their assigned roles (many-to-many)
        public DbSet<UserRole> UserRoles { get; set; }

        // This represents our Menus table - stores navigation menu items
        // Each menu item has a name, URL, and display order
        public DbSet<Menu> Menus { get; set; }

        // This represents the UserMenus junction table
        // It links users to their visible menu items (many-to-many)
        public DbSet<UserMenu> UserMenus { get; set; }

        // This method gets called when EF Core is building the database model
        // We use it to apply custom configurations for our entities
        // Instead of cluttering this class with all the rules, we split them into separate configuration classes
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply the configuration rules we defined in StaffConfiguration class
            // This includes things like table names, column sizes, indexes, etc.
            modelBuilder.ApplyConfiguration(new StaffConfiguration());
            
            // Apply the configuration rules for Appointments
            // This includes relationships, required fields, and column types
            modelBuilder.ApplyConfiguration(new AppointmentConfiguration());

            // Apply the configuration rules for Users
            // This includes username uniqueness, password hash storage, etc.
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            // Apply the configuration rules for Roles
            // This includes role name uniqueness and constraints
            modelBuilder.ApplyConfiguration(new RoleConfiguration());

            // Apply the configuration rules for UserRoles junction table
            // This sets up the many-to-many relationship between Users and Roles
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());

            // Apply the configuration rules for Menus
            // This includes menu name, URL, and display order settings
            modelBuilder.ApplyConfiguration(new MenuConfiguration());

            // Apply the configuration rules for UserMenus junction table
            // This sets up the many-to-many relationship between Users and Menus
            modelBuilder.ApplyConfiguration(new UserMenuConfiguration());
        }
    }
}
