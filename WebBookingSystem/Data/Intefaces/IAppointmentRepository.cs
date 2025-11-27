using WebBookingSystem.Data.Entities;

namespace WebBookingSystem.Data.Intefaces
{
    public interface IAppointmentRepository: IGenericRepository<Appointment>
    {
        IQueryable<Appointment> GetAppointmentsByUser(int userId);
        IQueryable<Appointment> GetUpcomingAppointments();
        IQueryable<Appointment> GetAppointmentsByStatus(AppointmentStatus status);
    }
}
