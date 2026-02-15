using AppointmentSystem.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Web.Data
{
    /// Configuration for the UserRole junction entity
    /// This sets up the many-to-many relationship between Users and Roles
    /// Each row in this table means "this user has this role"
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            // Tell EF Core this table should be named "UserRoles" in the database
            builder.ToTable("UserRoles");

            // Define Id as the primary key
            builder.HasKey(ur => ur.Id);

            // Set up the relationship: Each UserRole belongs to one User
            // - HasOne: Each UserRole record points to one User
            // - WithMany: Each User can have many UserRole records (many roles)
            // - HasForeignKey: UserId is the foreign key column
            // - OnDelete(Cascade): If we delete a user, delete their role assignments too
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Set up the relationship: Each UserRole belongs to one Role
            // - HasOne: Each UserRole record points to one Role
            // - WithMany: Each Role can have many UserRole records (many users)
            // - HasForeignKey: RoleId is the foreign key column
            // - OnDelete(Cascade): If we delete a role, delete its user assignments too
            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create a unique index on UserId + RoleId combination
            // This prevents assigning the same role to the same user twice
            builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();
        }
    }
}
