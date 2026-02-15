using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.Models
{
    /// View model used by the admin Users controller for creating and editing users
    /// Includes role and menu assignment checkboxes alongside the basic user fields
    public class UserFormViewModel
    {
        public Guid Id { get; set; }

        [DisplayName("Full Name")]
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [DisplayName("Username")]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        // Required when creating a new user, optional when editing
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        [DisplayName("Active")]
        public bool IsActive { get; set; } = true;

        // Checkboxes for role assignments
        public List<RoleAssignment> Roles { get; set; } = new();

        // Checkboxes for menu assignments
        public List<MenuAssignment> Menus { get; set; } = new();
    }

    /// Represents a single role checkbox on the user form
    public class RoleAssignment
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    /// Represents a single menu checkbox on the user form
    public class MenuAssignment
    {
        public Guid MenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
