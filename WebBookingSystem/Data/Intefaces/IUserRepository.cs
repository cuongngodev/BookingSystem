using BookingSystem.Data.Entities;

namespace WebBookingSystem.Data.Intefaces
{
    public interface IUserRepository : IGenericRepository<ApplicationUser>
    {
        IQueryable<ApplicationUser> GetAllUsers();
        ApplicationUser? GetUserById(int id);
    }

}
