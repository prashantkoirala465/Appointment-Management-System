using AppointmentSystem.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Web.Data
{
    /// Configuration for the User entity
    /// This tells Entity Framework Core exactly how to map our User class to a database table
    /// We use Fluent API here to keep our model class clean and focused on business logic
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Tell EF Core this table should be named "Users" in the database
            builder.ToTable("Users");

            // Define Id as the primary key
            // Every user needs a unique identifier
            builder.HasKey(u => u.Id);

            // Configure the FullName column
            // - Required: we can't have a user without a name
            // - Max 100 characters to prevent excessively long names
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            // Configure the Username column
            // - Required: needed for login authentication
            // - Max 50 characters is plenty for a username
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            // Configure the Email column
            // - Optional: not every user needs an email
            // - Max 255 characters to accommodate long email addresses
            builder.Property(u => u.Email)
                .HasMaxLength(255);

            // Configure the PasswordHash column
            // - Required: every user must have a password
            // - Max 255 characters for the hashed password
            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            // CreatedAtUtc tracks when the user account was created
            // This is audit trail data - always required
            builder.Property(u => u.CreatedAtUtc)
                .IsRequired();

            // Create a unique index on Username
            // This ensures no two users can have the same username
            // Also speeds up login queries since we search by username
            builder.HasIndex(u => u.Username)
                .IsUnique();

            // Create an index on Email for faster lookups
            builder.HasIndex(u => u.Email);
        }
    }
}
