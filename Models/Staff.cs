using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.Models
{
    /// Represents a staff member who can take appointments
    /// This could be a doctor, consultant, therapist, or any service provider
    public class Staff
    {
        // Unique identifier for each staff member
        // Using GUID means we can merge data from different sources without ID conflicts
        public Guid Id { get; set; }

        // The staff member's full name
        // [DisplayName] changes how this appears in forms ("Full Name" instead of "FullName")
        // This is required - we can't have a staff member without a name!
        [DisplayName("Full Name")]
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        // Contact information for the staff member
        // Email is optional but validated if provided
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        // Phone number is also optional
        // [Phone] ensures it's in a valid phone format if entered
        [DisplayName("Phone Number")]
        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        // What does this staff member specialize in?
        // Examples: "General Medicine", "Pediatrics", "Physical Therapy"
        // This helps clients choose the right person for their needs
        [StringLength(80)]
        public string? Specialty { get; set; }

        // Is this staff member currently active?
        // We don't delete staff records (to preserve historical data)
        // Instead, we mark them as inactive when they leave
        // Defaults to true when creating a new staff member
        [DisplayName("Active")]
        public bool IsActive { get; set; } = true;

        // Navigation property: all appointments assigned to this staff member
        // This is a one-to-many relationship (one staff -> many appointments)
        // Entity Framework uses this to automatically load related appointments when needed
        public List<Appointment> Appointments { get; set; } = new();
    }
}
