using AppointmentSystem.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Web.Data
{
    /// This class contains all the database-specific rules for the Staff entity
    /// We use Fluent API here instead of attributes to keep our models clean
    /// It's like the blueprint that tells EF Core exactly how to create the Staffs table
    public class StaffConfiguration : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> builder)
        {
            // Tell EF Core this table should be named "Staffs" in the database
            // Without this, it might use the DbSet property name
            builder.ToTable("Staffs");

            // Define Id as the primary key (the unique identifier for each row)
            // This is what uniquely identifies each staff member
            builder.HasKey(s => s.Id);

            // Configure the FullName column
            // - IsRequired() means it can't be null (NOT NULL in SQL)
            // - HasMaxLength(100) limits it to 100 characters
            // This prevents someone from entering a 10,000 character name!
            builder.Property(s => s.FullName)
                .IsRequired()
                .HasMaxLength(100);

            // Email is optional but has a max length
            // We give it more room (255 chars) since emails can be long
            builder.Property(s => s.Email)
                .HasMaxLength(255);

            // Phone number is optional, max 20 characters
            // This accommodates international formats with country codes
            builder.Property(s => s.PhoneNumber)
                .HasMaxLength(20);

            // Specialty field is optional and limited to 100 characters
            // Enough for "Pediatric Cardiology" but not a whole essay
            builder.Property(s => s.Specialty)
                .HasMaxLength(100);

            // Create an index on Email for faster lookups
            // If we search by email often, this makes queries much faster
            // It's like the index in the back of a book
            builder.HasIndex(s => s.Email);
            
            // Create an index on FullName too
            // This speeds up searches and sorting by name
            builder.HasIndex(s => s.FullName);
        }
    }
}
