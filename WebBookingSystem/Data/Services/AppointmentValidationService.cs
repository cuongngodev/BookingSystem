using Microsoft.EntityFrameworkCore;
using WebBookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;
using WebBookingSystem.Data.Services;

namespace WebBookingSystem.Services
{
    public class AppointmentValidationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BusinessHours _businessHours;
        private readonly ILogger<AppointmentValidationService> _logger;

        public AppointmentValidationService(
            IUnitOfWork unitOfWork,
            ILogger<AppointmentValidationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _businessHours = new BusinessHours();
        }

        #region Validate that appointment is within business hours
        public (bool IsValid, string ErrorMessage) ValidateBusinessHours(DateTime appointmentTime)
        {
            // Check if it's a working day
            if (!_businessHours.IsWorkingDay(appointmentTime.DayOfWeek))
            {
                _logger.LogWarning("Appointment attempted on non-working day: {Day}", appointmentTime.DayOfWeek);
                return (false, $"We are closed on {appointmentTime.DayOfWeek}s. Please select a working day (Monday-Saturday).");
            }

            // Check if it's within working hours
            if (!_businessHours.IsWithinWorkingHours(appointmentTime.TimeOfDay))
            {
                _logger.LogWarning("Appointment attempted outside business hours: {Time}", appointmentTime.TimeOfDay);
                return (false, $"Please select a time between {_businessHours.GetBusinessHoursDisplay()}.");
            }

            return (true, string.Empty);
        }
        #endregion

        #region Check if appointment time conflicts with existing appointments
        public (bool HasConflict, string ErrorMessage) CheckTimeConflict(Appointment newAppointment)
        {
            var service = _unitOfWork.ServiceRepository.GetById(newAppointment.ServiceId);
            if (service == null)
            {
                _logger.LogError("Service not found: {ServiceId}", newAppointment.ServiceId);
                return (true, "Service not found.");
            }

            var newStart = newAppointment.AppointmentTime;
            var newEnd = newStart.AddMinutes(service.Duration);

            _logger.LogInformation("Checking conflicts for appointment from {Start} to {End}", newStart, newEnd);

            // Find conflicting appointments
            // Only Pending and Confirmed appointments occupy time slots
            var conflicts = _unitOfWork.AppointmentRepository
                .GetAll()
                .Where(a =>
                    a.Id != newAppointment.Id &&
                    (a.Status == AppointmentStatus.Pending ||
                     a.Status == AppointmentStatus.Confirmed))
                .Include(a => a.Service)
                .AsEnumerable()
                .Any(existing =>
                {
                    var existingStart = existing.AppointmentTime;
                    var existingEnd = existingStart.AddMinutes(existing.Service.Duration);

                    // Check for time overlap
                    bool overlaps = newStart < existingEnd && newEnd > existingStart;

                    if (overlaps)
                    {
                        _logger.LogWarning("Conflict found with appointment ID {Id} ({Start}-{End})",
                            existing.Id, existingStart, existingEnd);
                    }

                    return overlaps;
                });

            if (conflicts)
            {
                return (true, "This time slot is already booked. Please select another time.");
            }

            return (false, string.Empty);
        }
        #endregion

        #region Validate that appointment is not in the past
        public (bool IsValid, string ErrorMessage) ValidateNotPastTime(DateTime appointmentTime)
        {
            if (appointmentTime <= DateTime.Now)
            {
                _logger.LogWarning("Appointment attempted for past time: {Time}", appointmentTime);
                return (false, "Cannot book appointments in the past. Please select a future date and time.");
            }

            return (true, string.Empty);
        }
        #endregion

        #region Perform all validations for an appointment
        public (bool IsValid, List<string> ErrorMessages) ValidateAppointment(Appointment appointment)
        {
            var errors = new List<string>();

            _logger.LogInformation("Validating appointment for service {ServiceId} at {Time}",
                appointment.ServiceId, appointment.AppointmentTime);

            // Validate not past time
            var (isNotPast, pastError) = ValidateNotPastTime(appointment.AppointmentTime);
            if (!isNotPast) errors.Add(pastError);

            // Validate business hours
            var (isBusinessHours, businessError) = ValidateBusinessHours(appointment.AppointmentTime);
            if (!isBusinessHours) errors.Add(businessError);

            // Check time conflicts (only if previous validations passed)
            if (errors.Count == 0)
            {
                var (hasConflict, conflictError) = CheckTimeConflict(appointment);
                if (hasConflict) errors.Add(conflictError);
            }

            if (errors.Count > 0)
            {
                _logger.LogWarning("Appointment validation failed with {Count} errors", errors.Count);
            }
            else
            {
                _logger.LogInformation("Appointment validation passed");
            }

            return (errors.Count == 0, errors);
        }

        #endregion

        #region Available Time Slots Generation
        public List<DateTime> GetAvailableTimeSlots(DateTime date, int serviceId)
        {
            _logger.LogInformation("Generating available slots for date {Date} and service {ServiceId}",
                date.Date, serviceId);

            var service = _unitOfWork.ServiceRepository.GetById(serviceId);
            if (service == null)
            {
                _logger.LogWarning("Service {ServiceId} not found", serviceId);
                return new List<DateTime>();
            }

            // Only show slots for future dates
            if (date.Date < DateTime.Today)
            {
                _logger.LogWarning("Requested date {Date} is in the past", date.Date);
                return new List<DateTime>();
            }

            // Check if it's a working day
            if (!_businessHours.IsWorkingDay(date.DayOfWeek))
            {
                _logger.LogInformation("Date {Date} is not a working day", date.Date);
                return new List<DateTime>();
            }

            // Generate all possible slots
            var allSlots = GenerateAllPossibleSlots(date);

            // Get occupied slots
            var occupiedSlots = GetOccupiedSlots(date);

            // Filter out occupied and past slots
            var now = DateTime.Now;
            var availableSlots = allSlots
                .Where(slot =>
                    slot > now &&  // Must be in the future
                    !IsSlotOccupied(slot, service.Duration, occupiedSlots))
                .ToList();

            _logger.LogInformation("Found {Count} available slots for {Date}",
                availableSlots.Count, date.Date);

            return availableSlots;
        }

        private List<DateTime> GenerateAllPossibleSlots(DateTime date)
        {
            var slots = new List<DateTime>();
            var currentSlot = date.Date.Add(_businessHours.OpenTime);
            var endTime = date.Date.Add(_businessHours.CloseTime);

            // Generate slots every 30 minutes
            while (currentSlot < endTime)
            {
                slots.Add(currentSlot);
                currentSlot = currentSlot.AddMinutes(30);
            }

            _logger.LogInformation("Generated {Count} possible slots for {Date}",
                slots.Count, date.Date);

            return slots;
        }
        private List<(DateTime Start, DateTime End)> GetOccupiedSlots(DateTime date)
        {
            var occupiedSlots = _unitOfWork.AppointmentRepository
                .GetAll()
                .Where(a =>
                    a.AppointmentTime.Date == date.Date &&
                    (a.Status == AppointmentStatus.Pending ||
                     a.Status == AppointmentStatus.Confirmed))
                .Include(a => a.Service)
                .AsEnumerable()
                .Select(a => (
                    Start: a.AppointmentTime,
                    End: a.AppointmentTime.AddMinutes(a.Service.Duration)
                ))
                .ToList();

            _logger.LogInformation("Found {Count} occupied slots for {Date}",
                occupiedSlots.Count, date.Date);

            return occupiedSlots;
        }

        private bool IsSlotOccupied(DateTime slotStart,int serviceDuration,List<(DateTime Start, DateTime End)> occupiedSlots)
        {
            var slotEnd = slotStart.AddMinutes(serviceDuration);

            // Check if this slot overlaps with any occupied slot
            return occupiedSlots.Any(occupied =>
                slotStart < occupied.End && slotEnd > occupied.Start
            );
        }

        #endregion
    }
}