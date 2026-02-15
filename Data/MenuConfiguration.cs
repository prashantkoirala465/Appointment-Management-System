using AppointmentSystem.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Web.Data
{
    /// Configuration for the Menu entity
    /// Defines how the Menus table is structured in the database
    /// Menus represent navigation items that can be assigned to specific users
    public class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            // Tell EF Core this table should be named "Menus" in the database
            builder.ToTable("Menus");

            // Define Id as the primary key
            builder.HasKey(m => m.Id);

            // Configure the MenuName column
            // - Required: every menu item needs a display name
            // - Max 100 characters is plenty for menu names
            builder.Property(m => m.MenuName)
                .IsRequired()
                .HasMaxLength(100);

            // Configure the Url column
            // - Required: every menu item needs a link/route
            // - Max 255 characters for the URL path
            builder.Property(m => m.Url)
                .IsRequired()
                .HasMaxLength(255);

            // DisplayOrder determines the position in the navigation bar
            // Required so we always know where to place each menu item
            builder.Property(m => m.DisplayOrder)
                .IsRequired();

            // Create an index on DisplayOrder for efficient sorting
            // When rendering the navigation bar, we sort by this field
            builder.HasIndex(m => m.DisplayOrder);
        }
    }
}
