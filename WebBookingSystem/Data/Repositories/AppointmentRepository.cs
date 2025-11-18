using WebBookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;

namespace WebBookingSystem.Data.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        private readonly ILogger<AppointmentRepository> _logger;
        public AppointmentRepository(ApplicationDbContext context, ILogger<AppointmentRepository> logger) : base(context, logger)
        {
            _logger = logger;
        }

        public IEnumerable<Appointment> GetAppointmentsByUser(int userId)
        {
            _logger.LogInformation("Fetching appointments for User {UserId}", userId);
            return _dbSet.Where(a => a.UserId == userId).ToList();
        }

        public IEnumerable<Appointment> GetUpcomingAppointments()
        {
            _logger.LogInformation("Fetching upcoming appointments");
            return _dbSet.Where(a => a.AppointmentTime > DateTime.Now).ToList();
        }
    }
}
