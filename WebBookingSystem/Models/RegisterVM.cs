using System.ComponentModel.DataAnnotations;

namespace WebBookingSystem.Models
{
    public class RegisterVM
    {
        [Required]
        [MinLength(2, ErrorMessage = "First Name must have at least 2 characters long.")]
        [RegularExpressionAttribute(@"^[^0-9]+$", ErrorMessage = "First Name cannot contain numbers.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Last Name must have at least 2 characters long.")]
        [RegularExpressionAttribute(@"^[^0-9]+$", ErrorMessage = "Last Name cannot contain numbers.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
        public string Password { get; set; }

        public DateTime DateJoined { get; set; } = DateTime.Now;

    }
}
