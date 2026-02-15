using AppointmentSystem.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Web.Data
{
    /// Configuration for the UserMenu junction entity
    /// This sets up the many-to-many relationship between Users and Menus
    /// Each row in this table means "this user can see this menu item"
    public class UserMenuConfiguration : IEntityTypeConfiguration<UserMenu>
    {
        public void Configure(EntityTypeBuilder<UserMenu> builder)
        {
            // Tell EF Core this table should be named "UserMenus" in the database
            builder.ToTable("UserMenus");

            // Define Id as the primary key
            builder.HasKey(um => um.Id);

            // Set up the relationship: Each UserMenu belongs to one User
            // - HasOne: Each UserMenu record points to one User
            // - WithMany: Each User can have many UserMenu records (many menus)
            // - HasForeignKey: UserId is the foreign key column
            // - OnDelete(Cascade): If we delete a user, delete their menu assignments too
            builder.HasOne(um => um.User)
                .WithMany(u => u.UserMenus)
                .HasForeignKey(um => um.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Set up the relationship: Each UserMenu belongs to one Menu
            // - HasOne: Each UserMenu record points to one Menu
            // - WithMany: Each Menu can have many UserMenu records (many users)
            // - HasForeignKey: MenuId is the foreign key column
            // - OnDelete(Cascade): If we delete a menu, delete its user assignments too
            builder.HasOne(um => um.Menu)
                .WithMany(m => m.UserMenus)
                .HasForeignKey(um => um.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create a unique index on UserId + MenuId combination
            // This prevents assigning the same menu to the same user twice
            builder.HasIndex(um => new { um.UserId, um.MenuId })
                .IsUnique();
        }
    }
}
