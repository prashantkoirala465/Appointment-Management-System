using AppointmentSystem.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Web.Data
{
    /// Configuration for the Role entity
    /// Defines how the Roles table is structured in the database
    /// Roles like "Admin", "Staff", "User" control access throughout the application
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // Tell EF Core this table should be named "Roles" in the database
            builder.ToTable("Roles");

            // Define Id as the primary key
            builder.HasKey(r => r.Id);

            // Configure the RoleName column
            // - Required: a role must have a name
            // - Max 50 characters is plenty for role names
            builder.Property(r => r.RoleName)
                .IsRequired()
                .HasMaxLength(50);

            // Description is optional but limited to 200 characters
            // Enough for a brief explanation of the role's purpose
            builder.Property(r => r.Description)
                .HasMaxLength(200);

            // Create a unique index on RoleName
            // This ensures no two roles can have the same name
            // For example, we can't accidentally create two "Admin" roles
            builder.HasIndex(r => r.RoleName)
                .IsUnique();
        }
    }
}
