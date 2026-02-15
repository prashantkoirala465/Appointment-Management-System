using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.Models
{
    /// Represents a role in the system (e.g., Admin, Staff, User)
    /// Roles define what level of access a user has within the application
    /// A role can be assigned to many users through the UserRole junction table
    public class Role
    {
        // Unique identifier for each role
        // Using GUID for globally unique, secure identifiers
        public Guid Id { get; set; }

        // The name of the role (e.g., "Admin", "Staff", "User")
        // This is required and must be unique
        [DisplayName("Role Name")]
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        // A brief description of what this role can do
        // Helps administrators understand the purpose of each role
        [StringLength(200)]
        public string? Description { get; set; }

        // Is this role currently active?
        // Inactive roles won't be available for assignment
        [DisplayName("Active")]
        public bool IsActive { get; set; } = true;

        // --- Navigation Properties ---

        // All users who have been assigned this role through the UserRole junction table
        // This is a many-to-many relationship (one role -> many users)
        public List<UserRole> UserRoles { get; set; } = new();
    }
}
