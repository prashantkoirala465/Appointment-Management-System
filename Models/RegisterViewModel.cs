using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.Models
{
    /// View model for the staff registration form (Ananta's module)
    /// This is what gets submitted when someone fills out the Register page
    /// Staff who register start with IsApproved = false — they can't log in
    /// until an admin approves them from the Users page
    public class RegisterViewModel
    {
        // the person's real name — shown in the sidebar and user listings
        [DisplayName("Full Name")]                                      // label text in the form
        [Required(ErrorMessage = "Full name is required")]               // server + client validation
        [StringLength(100)]                                              // max 100 characters
        public string FullName { get; set; } = string.Empty;

        // their chosen login username — must be unique in the system
        [DisplayName("Username")]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]                                               // max 50 characters
        public string Username { get; set; } = string.Empty;

        // email is optional during registration
        [EmailAddress]                                                   // validates email format if provided
        [StringLength(255)]
        public string? Email { get; set; }

        // password — gets SHA-256 hashed before storing in the database
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]                                    // renders as password input (dots)
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        // must match the Password field — prevents typos
        [DisplayName("Confirm Password")]
        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]   // cross-field validation
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
