using BookingSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using WebBookingSystem.Data.Intefaces;

namespace WebBookingSystem.Data.Repositories
{
    public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
            : base(context, logger)
        {
            _logger = logger;
        }

        #region Get All Users
        public IQueryable<ApplicationUser> GetAllUsers()
        {
            _logger.LogInformation("Fetching all users.");
            return _dbSet.AsQueryable();
        }
        #endregion

        #region Get User By Id
        public ApplicationUser? GetUserById(int id)
        {
            _logger.LogInformation("Fetching user with ID {Id}.", id);

            return _dbSet
                .Include(u => u.Appointments)          
                    .ThenInclude(a => a.Service)      
                .FirstOrDefault(u => u.Id == id);
        }
        #endregion
    }
}
