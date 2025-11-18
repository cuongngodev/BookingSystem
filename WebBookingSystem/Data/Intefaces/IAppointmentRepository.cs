using WebBookingSystem.Data.Entities;

namespace WebBookingSystem.Data.Intefaces
{
    public interface IAppointmentRepository: IGenericRepository<Appointment>
    {
        IEnumerable<Appointment> GetAppointmentsByUser(int userId);
        IEnumerable<Appointment> GetUpcomingAppointments();
        IEnumerable<Appointment> GetAppointmentsByStatus(AppointmentStatus status);
    }
}
