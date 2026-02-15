using System;

namespace AppointmentSystem.Web.Models
{
    /// Junction table that links Users to Roles
    /// This creates a many-to-many relationship between Users and Roles
    /// For example: User "John" can have roles "Admin" and "Staff"
    /// And the role "Admin" can be assigned to multiple users
    public class UserRole
    {
        // Unique identifier for each user-role assignment
        public Guid Id { get; set; }

        // Foreign key linking to the User table
        // This tells us which user has the role
        public Guid UserId { get; set; }

        // Navigation property to access the full User details
        // The ? means it can be null temporarily (like during creation)
        public User? User { get; set; }

        // Foreign key linking to the Role table
        // This tells us which role is assigned
        public Guid RoleId { get; set; }

        // Navigation property to access the full Role details
        public Role? Role { get; set; }
    }
}
