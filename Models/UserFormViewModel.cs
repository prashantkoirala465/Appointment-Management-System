using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.Models
{
    /// View model used by the admin Users controller for creating and editing users
    /// This is more complex than RegisterViewModel because it also includes
    /// role and menu assignment checkboxes (which only admins can control)
    /// The controller uses PopulateAssignments() to fill in the checkbox lists
    public class UserFormViewModel
    {
        // hidden field in the form — used during Edit to know which user we're updating
        // for Create, this is Guid.Empty (default)
        public Guid Id { get; set; }

        // --- Basic user fields (same as RegisterViewModel) ---

        [DisplayName("Full Name")]                                      // label text shown in the form
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [DisplayName("Username")]                                       // must be unique in the system
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [EmailAddress]                                                   // validates format if provided
        [StringLength(255)]
        public string? Email { get; set; }

        // password is REQUIRED when creating a new user
        // but OPTIONAL when editing — leaving it blank means "keep existing password"
        // the controller checks this manually in the Create action
        [DataType(DataType.Password)]                                    // renders as password dots
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }                            // nullable because it's optional on edit

        // toggle to enable/disable the account without deleting it
        [DisplayName("Active")]
        public bool IsActive { get; set; } = true;                       // defaults to active for new users

        // --- Role & Menu assignment lists ---
        // these get rendered as checkbox groups in the Create/Edit forms
        // the controller populates them via PopulateAssignments()

        // list of all available roles with checkboxes (IsSelected = ticked or not)
        public List<RoleAssignment> Roles { get; set; } = new();

        // list of all available menus with checkboxes
        public List<MenuAssignment> Menus { get; set; } = new();
    }

    /// Represents a single role checkbox on the user create/edit form
    /// The view loops through List<RoleAssignment> to render each checkbox
    public class RoleAssignment
    {
        public Guid RoleId { get; set; }                 // the role's primary key (sent as hidden field)
        public string RoleName { get; set; } = string.Empty;  // display text next to the checkbox
        public bool IsSelected { get; set; }             // whether the checkbox is ticked
    }

    /// Represents a single menu checkbox on the user create/edit form
    /// Same pattern as RoleAssignment — used for sidebar menu assignments
    public class MenuAssignment
    {
        public Guid MenuId { get; set; }                 // the menu's primary key
        public string MenuName { get; set; } = string.Empty;  // display text ("Appointments", "Staff", etc.)
        public bool IsSelected { get; set; }             // ticked = this user gets this sidebar item
    }
}
