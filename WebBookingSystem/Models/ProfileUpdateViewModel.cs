using System.ComponentModel.DataAnnotations;

namespace WebBookingSystem.Models
{
    public class ProfileUpdateViewModel
    {
        [Required]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters.")]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters.")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and contain uppercase, lowercase, number, and special character.")]
        public string? Password { get; set; }
    }
}
