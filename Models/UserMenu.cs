using System;

namespace AppointmentSystem.Web.Models
{
    /// Junction table that links Users to Menus
    /// This creates a many-to-many relationship between Users and Menus
    /// It determines which menu items are visible to each user
    /// For example: An "Admin" user might see all menus, while a "Staff" user only sees some
    public class UserMenu
    {
        // Unique identifier for each user-menu assignment
        public Guid Id { get; set; }

        // Foreign key linking to the User table
        // This tells us which user has access to the menu item
        public Guid UserId { get; set; }

        // Navigation property to access the full User details
        // The ? means it can be null temporarily (like during creation)
        public User? User { get; set; }

        // Foreign key linking to the Menu table
        // This tells us which menu item is assigned
        public Guid MenuId { get; set; }

        // Navigation property to access the full Menu details
        public Menu? Menu { get; set; }
    }
}
