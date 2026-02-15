using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.Models
{
    /// Represents a menu item (navigation link) in the application
    /// Menus are assigned to users through UserMenu to control what each user can see
    /// This allows dynamic, role-based navigation - different users see different menu items
    public class Menu
    {
        // Unique identifier for each menu item
        public Guid Id { get; set; }

        // The display name shown in the navigation bar
        // For example: "Appointments", "Staff Management", "Dashboard"
        [DisplayName("Menu Name")]
        [Required(ErrorMessage = "Menu name is required")]
        [StringLength(100)]
        public string MenuName { get; set; } = string.Empty;

        // The URL or route this menu item points to
        // For example: "/Appointments" or "/Staffs"
        [DisplayName("URL")]
        [Required(ErrorMessage = "URL is required")]
        [StringLength(255)]
        public string Url { get; set; } = string.Empty;

        // Controls the order in which menu items appear in the navigation
        // Lower numbers appear first (e.g., 1 = first, 2 = second)
        [DisplayName("Display Order")]
        public int DisplayOrder { get; set; }

        // Is this menu item currently active/visible?
        // Inactive menus won't be shown to any user
        [DisplayName("Active")]
        public bool IsActive { get; set; } = true;

        // --- Navigation Properties ---

        // All user-menu assignments for this menu item
        // This tells us which users have access to this menu
        public List<UserMenu> UserMenus { get; set; } = new();
    }
}
