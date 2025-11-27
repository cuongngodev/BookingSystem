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
        #region Get Appointments by User
        public IQueryable<Appointment> GetAppointmentsByUser(int userId)
        {
            _logger.LogInformation("Fetching appointments for user ID {UserId}.", userId);
            try
            {
                var appointments = _dbSet.Where(a => a.UserId == userId);
                _logger.LogInformation("Appointments fetched successfully for user ID {UserId}.", userId);
                return appointments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointments for user ID {UserId}.", userId);
                throw;
            }
        }
        #endregion

        #region Get Upcoming Appointments
        public IQueryable<Appointment> GetUpcomingAppointments()
        {
            _logger.LogInformation("Fetching upcoming appointments.");
            try
            {
                var appointments = _dbSet.Where(a => a.AppointmentTime > DateTime.Now);
                _logger.LogInformation("Upcoming appointments fetched successfully.");
                return appointments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching upcoming appointments.");
                throw;
            }
        }
        #endregion

        #region Get Appointments by Status
        public IQueryable<Appointment> GetAppointmentsByStatus(AppointmentStatus status)
        {
            _logger.LogInformation("Fetching appointments with status {Status}.", status);
            try
            {
                var appointments = _dbSet.Where(a => a.Status == status);
                _logger.LogInformation("Appointments fetched successfully for status {Status}.", status);
                return appointments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointments with status {Status}.", status);
                throw;
            }
        }
        #endregion
    }
}