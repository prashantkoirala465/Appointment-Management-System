using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.Models
{
    /// This view model is used specifically for the login form
    /// It's not a database entity - it only carries data between the login form and the controller
    /// We keep it separate from the User model because we don't need all user fields for login
    public class LoginViewModel
    {
        // The username entered by the user on the login form
        // Required because we can't authenticate without knowing who they are
        [DisplayName("Username")]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        // The password entered by the user on the login form
        // Required because we can't authenticate without a password
        // [DataType(Password)] tells the view to render this as a password field (dots instead of text)
        [DisplayName("Password")]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
