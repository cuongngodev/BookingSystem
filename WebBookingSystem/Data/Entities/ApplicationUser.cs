using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using WebBookingSystem.Data.Entities;

namespace BookingSystem.Data.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }
        public DateTime? DateJoined { get; set; } = DateTime.Now;

        public ICollection<Appointment>? Appointments {get; set;}
    }
}
