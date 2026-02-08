using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.Models
{
    /// This represents a single appointment in our system
    /// Think of it as a booking slot where a client meets with a staff member
    public class Appointment
    {
        // Every appointment gets a unique identifier (GUID)
        // We use GUID instead of integers because they're globally unique and more secure
        public Guid Id { get; set; }

        // This links the appointment to a specific staff member
        // The [Required] attribute means we can't create an appointment without assigning it to someone
        [Required]
        public Guid StaffId { get; set; }

        // This is the navigation property - it lets us easily access the staff member's full details
        // The ? means it can be null temporarily (like when we're first creating the appointment)
        public Staff? Staff { get; set; }

        // --- Client Information Section ---
        // These fields store who the appointment is for
        
        // The client's full name - required field, max 200 characters
        [Required]
        [StringLength(200)]
        public string ClientName { get; set; } = string.Empty;

        // Optional email for sending confirmations or reminders
        // [EmailAddress] validates that it's a proper email format
        [EmailAddress]
        [StringLength(255)]
        public string? ClientEmail { get; set; }

        // Client's phone number is required so we can reach them if needed
        // [Phone] validates that it looks like a real phone number
        [Required]
        [Phone]
        [StringLength(20)]
        public string ClientPhone { get; set; } = string.Empty;

        // --- Appointment Timing Section ---
        
        // When does this appointment start?
        // We store the exact date and time
        [Required]
        public DateTime StartTime { get; set; }

        // How long will this appointment last?
        // Stored in minutes (e.g., 30, 60, 90)
        // The range is 1-1440 minutes (1 minute to 24 hours max)
        [Required]
        [Range(1, 1440)]
        public int DurationMinutes { get; set; }

        // --- Status Tracking Section ---
        
        // Current status of the appointment
        // Common values: "Scheduled", "Completed", "Cancelled", "No-Show"
        // Defaults to "Scheduled" when first created
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        // Any special notes or instructions for this appointment
        // This is optional - staff might add notes about what to prepare or special requests
        [StringLength(500)]
        public string? Notes { get; set; }

        // --- Audit Trail Section ---
        // These help us track when appointments were created and modified
        
        // When was this appointment first created?
        // Automatically set to current UTC time when the appointment is made
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // When was this appointment last modified?
        // Null if it's never been edited, otherwise stores the last update time
        public DateTime? UpdatedAtUtc { get; set; }
    }
}