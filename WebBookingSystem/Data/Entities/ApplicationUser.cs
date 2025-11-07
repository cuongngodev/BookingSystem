using Microsoft.AspNetCore.Identity;

namespace BookingSystem.Data.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string? FullName { get; set; }

    }
}
