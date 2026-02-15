using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.Models
{
    /// Represents a user who can log into the system
    /// Users are authenticated and then authorized based on their assigned roles
    /// Each user can have multiple roles and multiple menu items assigned to them
    public class User
    {
        // Unique identifier for each user
        // Using GUID for globally unique, secure identifiers
        public Guid Id { get; set; }

        // The user's full name for display purposes
        // This is required - every user must have a name
        [DisplayName("Full Name")]
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        // The username used for logging into the system
        // Must be unique across all users
        [DisplayName("Username")]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        // The user's email address - optional but validated if provided
        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        // Hashed password for secure authentication
        // We never store plain text passwords!
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        // Is this user account currently active?
        // We don't delete user records - we deactivate them instead
        // Defaults to true when creating a new user
        [DisplayName("Active")]
        public bool IsActive { get; set; } = true;

        // When was this user account created?
        // Automatically set to current UTC time when the user is created
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---

        // All roles assigned to this user through the UserRole junction table
        // This is a many-to-many relationship (one user -> many roles)
        public List<UserRole> UserRoles { get; set; } = new();

        // All menus assigned to this user through the UserMenu junction table
        // This determines which menu items this user can see and access
        public List<UserMenu> UserMenus { get; set; } = new();
    }
}
