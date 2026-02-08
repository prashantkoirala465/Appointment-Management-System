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
        }
    }
}
