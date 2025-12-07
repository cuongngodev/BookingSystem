using WebBookingSystem.Data.Entities;

namespace WebBookingSystem.Models
{
    public class AppointmentVM
    {
        // Core identifiers
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int UserId { get; set; }

        // Appointment date & time (separate)
     
        public string SelectedAppointmentDateTime { get; set; } = string.Empty;

        // Editable fields
        public string? Notes { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }

        // --- Extra read-only display data ---
        // Client/User info
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;

        // Service info
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public int ServiceDuration { get; set; }

        // Optional for auditing
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
