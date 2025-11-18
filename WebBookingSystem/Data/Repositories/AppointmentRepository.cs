using WebBookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;

namespace WebBookingSystem.Data.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext context, ILogger<AppointmentRepository> logger) : base(context, logger)
        {
          
        }

        public IEnumerable<Appointment> GetAppointmentsByUser(int userId)
        {
            return _dbSet.Where(a => a.UserId == userId).ToList();
        }

        public IEnumerable<Appointment> GetUpcomingAppointments()
        {
            return _dbSet.Where(a => a.AppointmentTime > DateTime.Now).ToList();
        }

        public IEnumerable<Appointment> GetAppointmentsByStatus(AppointmentStatus status)
        {
            return _dbSet.Where(a => a.Status == status).ToList();
        }
    }
}
