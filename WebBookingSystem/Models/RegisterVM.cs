using System.ComponentModel.DataAnnotations;

namespace WebBookingSystem.Models
{
    public class RegisterVM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }

        public DateTime DateJoined { get; set; } = DateTime.Now;

    }
}
