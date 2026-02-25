using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// DTOs (Data Transfer Objects) are simple classes that define the shape of JSON
/// that goes in and out of our API endpoints.
/// Why not just use the entity models directly? Because:
///   1. We don't want to expose internal fields like PasswordHash
///   2. We can flatten nested relationships (e.g., Staff.FullName → StaffName)
///   3. We can add validation rules specific to API input
///   4. We control exactly which properties are sent/received

namespace AppointmentSystem.Web.Models.Api
{
    // ══════════════════════════════════════════════════════════
    //  APPOINTMENT DTOs (Prashant's module)
    // ══════════════════════════════════════════════════════════

    /// Response DTO — this is the shape of JSON returned when you GET an appointment
    /// Notice: StaffName is included here but not in the entity model — it's computed during projection
    public class AppointmentDto
    {
        public Guid Id { get; set; }                           // appointment unique identifier
        public Guid StaffId { get; set; }                      // FK to the assigned staff member
        public string StaffName { get; set; } = string.Empty;  // flattened from Staff.FullName
        public string ClientName { get; set; } = string.Empty; // who the appointment is for
        public string? ClientEmail { get; set; }               // nullable — email is optional
        public string ClientPhone { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }                // when the appointment starts
        public int DurationMinutes { get; set; }               // how long it lasts
        public string Status { get; set; } = string.Empty;     // "Scheduled", "Completed", "Cancelled"
        public string? Notes { get; set; }                     // optional notes from admin/staff
        public DateTime CreatedAtUtc { get; set; }             // when it was created
        public DateTime? UpdatedAtUtc { get; set; }            // when it was last modified (null if never)
    }

    /// Request DTO — this is the JSON shape the client sends when creating/updating an appointment
    /// Data annotations provide server-side validation (checked automatically by [ApiController])
    public class AppointmentCreateDto
    {
        [Required]                                 // must specify which staff member handles this
        public Guid StaffId { get; set; }

        [Required, StringLength(200)]              // client name is mandatory, max 200 chars
        public string ClientName { get; set; } = string.Empty;

        [EmailAddress, StringLength(255)]           // if provided, must be a valid email format
        public string? ClientEmail { get; set; }

        [Required, Phone, StringLength(20)]         // phone is required and must look like a phone number
        public string ClientPhone { get; set; } = string.Empty;

        [Required]                                 // must specify when the appointment is
        public DateTime StartTime { get; set; }

        [Required, Range(1, 1440)]                 // duration between 1 minute and 24 hours
        public int DurationMinutes { get; set; }

        [StringLength(50)]                         // status string, defaults to "Scheduled"
        public string Status { get; set; } = "Scheduled";

        [StringLength(500)]                        // optional notes, max 500 chars
        public string? Notes { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    //  STAFF DTOs (Anupam's module)
    // ══════════════════════════════════════════════════════════

    /// Response DTO for staff members — includes computed AppointmentCount
    public class StaffDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;     // staff member's display name
        public string? Email { get; set; }                       // contact email (optional)
        public string? PhoneNumber { get; set; }                 // contact phone (optional)
        public string? Specialty { get; set; }                   // e.g., "Cardiology", "Pediatrics"
        public bool IsActive { get; set; }                       // inactive staff can't get new appointments
        public int AppointmentCount { get; set; }                // how many appointments they have (computed)
    }

    /// Request DTO for creating/updating staff
    public class StaffCreateDto
    {
        [Required, StringLength(100)]              // name is required
        public string FullName { get; set; } = string.Empty;

        [EmailAddress, StringLength(255)]           // email format validation
        public string? Email { get; set; }

        [Phone, StringLength(20)]                  // phone format validation
        public string? PhoneNumber { get; set; }

        [StringLength(100)]                        // specialty text (optional)
        public string? Specialty { get; set; }

        public bool IsActive { get; set; } = true; // defaults to active when creating
    }

    // ══════════════════════════════════════════════════════════
    //  USER DTOs (Ananta's module)
    // ══════════════════════════════════════════════════════════

    /// Response DTO for users — notice we DON'T include PasswordHash
    /// Roles and Menus are flattened to simple string lists (no GUIDs needed in response)
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }                            // account enabled/disabled
        public bool IsApproved { get; set; }                          // false = pending admin approval
        public DateTime CreatedAtUtc { get; set; }
        public List<string> Roles { get; set; } = new();              // e.g., ["Admin", "Staff"]
        public List<string> Menus { get; set; } = new();              // e.g., ["Dashboard", "Appointments"]
    }

    /// Request DTO for creating a user through the API (admin only)
    /// Includes password (plain text — we hash it server-side) and assignment IDs
    public class UserCreateDto
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [EmailAddress, StringLength(255)]
        public string? Email { get; set; }

        [Required, MinLength(6)]                   // password must be at least 6 characters
        public string Password { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        /// GUIDs of roles to assign to this user (e.g., the Admin role's ID)
        public List<Guid> RoleIds { get; set; } = new();

        /// GUIDs of menus to assign (determines sidebar navigation)
        public List<Guid> MenuIds { get; set; } = new();
    }

    // ══════════════════════════════════════════════════════════
    //  ROLE DTOs (Ananta's module)
    // ══════════════════════════════════════════════════════════

    /// Response DTO for roles — includes how many users have this role
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;         // e.g., "Admin", "Staff"
        public string? Description { get; set; }                     // human-readable description
        public bool IsActive { get; set; }                           // inactive roles can't be assigned
        public int UserCount { get; set; }                           // computed: how many users have this
    }

    /// Request DTO for creating/updating a role
    public class RoleCreateDto
    {
        [Required, StringLength(50)]               // role name is required, max 50 chars
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]                        // optional description
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true; // defaults to active
    }

    // ══════════════════════════════════════════════════════════
    //  MENU DTOs (Ananta's module)
    // ══════════════════════════════════════════════════════════

    /// Response DTO for menu items — what shows in the sidebar
    public class MenuDto
    {
        public Guid Id { get; set; }
        public string MenuName { get; set; } = string.Empty;         // display text ("Appointments")
        public string Url { get; set; } = string.Empty;              // link target ("/Appointments")
        public int DisplayOrder { get; set; }                        // sort position in sidebar
        public bool IsActive { get; set; }                           // hidden if inactive
    }

    /// Request DTO for creating/updating a menu item
    public class MenuCreateDto
    {
        [Required, StringLength(100)]              // menu display name
        public string MenuName { get; set; } = string.Empty;

        [Required, StringLength(255)]              // the URL this menu links to
        public string Url { get; set; } = string.Empty;

        [Required]                                 // sort order is mandatory
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true; // defaults to active/visible
    }

    // ══════════════════════════════════════════════════════════
    //  AUTH DTOs (Ananta's module)
    // ══════════════════════════════════════════════════════════

    /// Request DTO for API login — sent as JSON in POST body
    public class LoginDto
    {
        [Required]                                 // must provide username
        public string Username { get; set; } = string.Empty;

        [Required]                                 // must provide password
        public string Password { get; set; } = string.Empty;
    }

    /// Response DTO returned after a successful login
    /// Contains user info + a JWT token for subsequent API calls
    public class LoginResponseDto
    {
        public Guid UserId { get; set; }                             // the logged-in user's ID
        public string Username { get; set; } = string.Empty;         // their username
        public string FullName { get; set; } = string.Empty;         // display name
        public List<string> Roles { get; set; } = new();             // assigned roles
        public string Message { get; set; } = string.Empty;          // human-readable status message

        /// The JWT Bearer token — client sends this in the Authorization header
        /// Format: "Authorization: Bearer eyJhbGci..."
        /// Only included in login response, not in /me
        public string? Token { get; set; }

        /// When the token expires — client should request a new one before this time
        public DateTime? ExpiresAt { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    //  DASHBOARD DTO (Anupam's module)
    // ══════════════════════════════════════════════════════════

    /// Response DTO for the dashboard overview — packed with aggregated statistics
    /// All these counts are computed by running COUNT queries against the database
    public class DashboardDto
    {
        // --- Appointment stats ---
        public int TotalAppointments { get; set; }         // all appointments ever created
        public int ScheduledAppointments { get; set; }     // status = "Scheduled" (upcoming)
        public int CompletedAppointments { get; set; }     // status = "Completed" (done)
        public int CancelledAppointments { get; set; }     // status = "Cancelled"
        public int TodayAppointments { get; set; }         // appointments happening today

        // --- Staff stats ---
        public int TotalStaff { get; set; }                // all staff members
        public int ActiveStaff { get; set; }               // only active (IsActive = true)

        // --- User stats ---
        public int TotalUsers { get; set; }                // all user accounts
        public int ActiveUsers { get; set; }               // enabled accounts
        public int PendingApprovals { get; set; }          // staff registrations awaiting admin approval

        // --- Recent activity ---
        public List<AppointmentDto> RecentAppointments { get; set; } = new(); // last 5 appointments created
    }
}
