using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.Models.Api
{
    // ══════════════════════════════════════════════════════════
    //  APPOINTMENT DTOs
    // ══════════════════════════════════════════════════════════

    /// Response DTO returned when fetching appointments
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string? ClientEmail { get; set; }
        public string ClientPhone { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    /// Request DTO for creating or updating an appointment
    public class AppointmentCreateDto
    {
        [Required]
        public Guid StaffId { get; set; }

        [Required, StringLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [EmailAddress, StringLength(255)]
        public string? ClientEmail { get; set; }

        [Required, Phone, StringLength(20)]
        public string ClientPhone { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required, Range(1, 1440)]
        public int DurationMinutes { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    //  STAFF DTOs
    // ══════════════════════════════════════════════════════════

    /// Response DTO returned when fetching staff members
    public class StaffDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Specialty { get; set; }
        public bool IsActive { get; set; }
        public int AppointmentCount { get; set; }
    }

    /// Request DTO for creating or updating a staff member
    public class StaffCreateDto
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress, StringLength(255)]
        public string? Email { get; set; }

        [Phone, StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Specialty { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // ══════════════════════════════════════════════════════════
    //  USER DTOs
    // ══════════════════════════════════════════════════════════

    /// Response DTO returned when fetching users
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Menus { get; set; } = new();
    }

    /// Request DTO for creating a user (admin only)
    public class UserCreateDto
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [EmailAddress, StringLength(255)]
        public string? Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        /// List of Role IDs to assign
        public List<Guid> RoleIds { get; set; } = new();

        /// List of Menu IDs to assign
        public List<Guid> MenuIds { get; set; } = new();
    }

    // ══════════════════════════════════════════════════════════
    //  ROLE DTOs
    // ══════════════════════════════════════════════════════════

    /// Response DTO returned when fetching roles
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int UserCount { get; set; }
    }

    /// Request DTO for creating or updating a role
    public class RoleCreateDto
    {
        [Required, StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // ══════════════════════════════════════════════════════════
    //  MENU DTOs
    // ══════════════════════════════════════════════════════════

    /// Response DTO returned when fetching menus
    public class MenuDto
    {
        public Guid Id { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    /// Request DTO for creating or updating a menu
    public class MenuCreateDto
    {
        [Required, StringLength(100)]
        public string MenuName { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Url { get; set; } = string.Empty;

        [Required]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // ══════════════════════════════════════════════════════════
    //  AUTH DTOs
    // ══════════════════════════════════════════════════════════

    /// Request DTO for API login
    public class LoginDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    /// Response DTO after successful login
    public class LoginResponseDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string Message { get; set; } = string.Empty;

        /// JWT token for API authentication (only returned on login)
        public string? Token { get; set; }

        /// Token expiration time in UTC
        public DateTime? ExpiresAt { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    //  DASHBOARD DTO
    // ══════════════════════════════════════════════════════════

    /// Response DTO for dashboard statistics
    public class DashboardDto
    {
        public int TotalAppointments { get; set; }
        public int ScheduledAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int TotalStaff { get; set; }
        public int ActiveStaff { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int PendingApprovals { get; set; }
        public List<AppointmentDto> RecentAppointments { get; set; } = new();
    }
}
