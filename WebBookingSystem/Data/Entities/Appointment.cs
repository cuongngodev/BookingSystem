using BookingSystem.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBookingSystem.Data.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ServiceId { get; set; }

        public DateTime AppointmentTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

    }

    public enum AppointmentStatus
    {
        Pending = 0,
        Confirmed = 1,
        Completed = 2,
        Cancelled = 3,
    }
}
