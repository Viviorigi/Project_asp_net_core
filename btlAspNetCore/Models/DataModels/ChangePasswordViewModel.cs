using System.ComponentModel.DataAnnotations;

namespace btlAspNetCore.Models.DataModels
{
    public class ChangePasswordViewModel
    {
        public int UserId { get; set; } // Hidden field for the user ID

        [Required(ErrorMessage = "Old password is required")]
        [DataType(DataType.Password)]
        public string PasswordOld { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm the new password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The passwords do not match")]
        public string Repassword { get; set; }
    }
}
