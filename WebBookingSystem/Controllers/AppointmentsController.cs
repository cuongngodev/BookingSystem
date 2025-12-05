using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;
using WebBookingSystem.Models;
using WebBookingSystem.Services;

namespace WebBookingSystem.Controllers
{
    [Authorize] // all actions require a logged-in user

    public class AppointmentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AppointmentsController> _logger;
        private readonly AppointmentValidationService _appointmentValidationService;

        public AppointmentsController(
            IUnitOfWork unitOfWork, 
            ILogger<AppointmentsController> logger,
            AppointmentValidationService appointmentValidationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _appointmentValidationService = appointmentValidationService;
        }
        /// <summary>
        /// Returns a calender view of appointments, another interface for admin to manage and perform different action to appointments.
        /// </summary>
       
        public IActionResult Manage()
        {
            return View(); 
        }
        /// <summary>
        /// Retrieves a list of appointment events with associated service information for use in calendar or scheduling
        /// interfaces.
        /// </summary>
        /// <remarks>Each event object includes the appointment ID, service name, start and end times, and
        /// an indicator that the event is not all-day. This endpoint event data is used for calender view. The returned data is read-only and does not
        /// include sensitive information.</remarks>
        /// <returns>A JSON result containing a collection of event objects, each with appointment and service details. The
        /// collection will be empty if no appointments are found.</returns>
        [HttpGet]
        public IActionResult GetEvents()
        {
            // Return strongly-typed DateTime values and avoid formatting inside the EF projection.
            var events = _unitOfWork.AppointmentRepository
                    .GetAll()
                    .Include(a => a.Service)
                    .Include(a => a.Client) 
                    .AsNoTracking()
                    .Select(a => new
                    {
                        id = a.Id,
                        title = a.Service != null ? a.Service.Name : "Unknown",
                        start = a.AppointmentTime,
                        end = a.Service != null ? a.AppointmentTime.AddMinutes(a.Service.Duration) : a.AppointmentTime,
                        notes = a.Notes,
                        status = a.Status.ToString(),
                        userName = a.Client != null ? a.Client.FirstName + a.Client.LastName : "N/A", 
                        userPhone = a.Client != null ? a.Client.PhoneNumber : "N/A",
                        serviceId = a.ServiceId,
                        serviceName = a.Service.Name,
                        email = a.Client !=null ? a.Client.Email : "N/A",
                        allDay = false
                    })
                    .ToList();

            return Json(events);
        }

        /// <summary>
        /// Processes an appointment update request submitted from the calendar interface.
        /// </summary>
        /// <remarks>This action requires the user to have either the Admin or Employee role. Only
        /// administrators can change the appointment status. The service and user associated with the appointment
        /// cannot be modified through this method.</remarks>
        /// <param name="model">An <see cref="Appointment"/> object containing the updated appointment details. The <c>Id</c> property must
        /// correspond to an existing appointment.</param>
        /// <returns>A JSON result indicating success if the update is completed; otherwise, a view displaying the appointment if
        /// an error occurs, or a NotFound result if the appointment does not exist.</returns>
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditFromCalendar(AppointmentEditVM model)
        {
            var appointment = _unitOfWork.AppointmentRepository.GetById(model.Id);
            if (appointment == null)
            {
                return NotFound();
            }
            try
            {
                if (DateTime.TryParse(model.SelectedAppointmentDateTime, out DateTime parsedDateTime))
                {
                    // Keep it as local time
                    appointment.AppointmentTime = DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Local);
                }
                else
                {
                    _logger.LogWarning("Invalid datetime input: {Date}", model.SelectedAppointmentDateTime);
                }

                appointment.Notes = model.Notes;
                appointment.Status = model.Status;
                appointment.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.AppointmentRepository.Update(appointment);
                _unitOfWork.AppointmentRepository.SaveAll();

                _logger.LogInformation("Appointment {Id} updated successfully by Admin", model.Id);
                return RedirectToAction("Manage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment ID {Id}.", model.Id);
                return RedirectToAction("Manage");
            }

        }

        #region GET: Appointments (Admin)
        [Authorize(Roles = "Admin,Employee,Customer")] // allow all role to visit, but different data
        [HttpGet]
        public IActionResult Index()
        {
            IEnumerable<Appointment> appointments;

            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                // admin and employee can see all appointments
                _logger.LogInformation("Fetching all appointments for admin.");
                appointments = _unitOfWork.AppointmentRepository
                    .GetAll()
                    .Include(a => a.Service)
                    //.Include(a => a.User)
                    .OrderByDescending(a => a.AppointmentTime)
                    .ToList();
            }
            else
            {
                // customer can only own appoinments
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                _logger.LogInformation($"Fetching appointments for customer {userId}.");
                appointments = _unitOfWork.AppointmentRepository
                    .GetAll()
                    .Include(a => a.Service)
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.AppointmentTime)
                    .ToList();
            }
            _logger.LogInformation("Returned {Count} appointments.", appointments.Count());
            return View(appointments);
        }
        #endregion

        #region GET: Appointments/Details/{id}
        [Authorize(Roles = "Customer,Admin,Employee")]
        public IActionResult Details(int? id)
        {
            _logger.LogInformation($"Fetching appointments details for ID: {id}");

            if (id == null)
            {
                _logger.LogWarning("Details request failed: ID was null.");
                return BadRequest();
            }

            var appointment = _unitOfWork.AppointmentRepository
                .GetAll()
                .Include(a => a.Service)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment with ID {Id} not found.", id);
                return NotFound();
            }
            return View(appointment);
        }
        #endregion

        #region GET: Appointments/Create (From Service)
        [Authorize(Roles = "Customer,Admin,Employee")]
        public IActionResult Create(int serviceId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);           
            _logger.LogInformation($"Opening create appointment page for service ID: {serviceId}");

            // Load service 
            var service = _unitOfWork.ServiceRepository
                .GetAll()
                .FirstOrDefault(s => s.Id == serviceId);

            if (service == null)
            {
                return NotFound("Service not found.");
            }

            // Pass service to the view
            ViewBag.ServiceName = service.Name;
            ViewBag.Duration = service.Duration;

            // Set default appointment time (at least 1 day in advance)
            var defaultTime = DateTime.Today.AddDays(1).AddHours(10);

            var appointment = new Appointment
            {
                ServiceId = serviceId,
                UserId = int.Parse(userIdString), // actual user
                Status = AppointmentStatus.Pending,
                AppointmentTime = defaultTime
            };

            return View(appointment);
        }
        #endregion

        #region POST: Appointments/Create
        [HttpPost]
        [Authorize(Roles = "Customer,Admin,Employee")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
            _logger.LogInformation($"Attempting to create new appointment for user {appointment.UserId}.");

            // actual userId to make an appoinment
            appointment.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            appointment.Status = AppointmentStatus.Pending;
            appointment.UpdatedAt = DateTime.Now;

            _logger.LogInformation($"Attempting to create new appointment for user {appointment.UserId}.");

            if (ModelState.IsValid)
            {
                _unitOfWork.AppointmentRepository.Add(appointment);
                _unitOfWork.AppointmentRepository.SaveAll();
                _logger.LogInformation($"Appointment successfully created with ID: {appointment.Id}.");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogWarning("Appointment creation failed due to invalid model state.");
                return View(appointment);
            }
        }
        #endregion

        #region GET: Appointments/Edit/{id}
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Edit(int id)
        {
            _logger.LogInformation($"Fetching appointment for update (ID: {id}).");
            var appointment = _unitOfWork.AppointmentRepository
                .GetAll()
                .Include(a => a.Service)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                _logger.LogWarning("Edit failed: Appointment with ID {Id} not found.", id);
                return NotFound();
            }

            return View(appointment);
        }
        #endregion

        #region POST: Appointments/Edit/5
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id)
            {
                _logger.LogWarning("Edit request ID mismatch. Expected {Id}, received {ReceivedId}.", id, appointment.Id);
                return BadRequest();
            }
            try
            {
                _logger.LogInformation("Attempting to update appointment ID {Id}.", id);
                // only admin can change status
                // user cannot change serviceId or userId
                appointment.UpdatedAt = DateTime.Now;

                if (ModelState.IsValid)
                {
                    _unitOfWork.AppointmentRepository.Update(appointment);
                    _unitOfWork.AppointmentRepository.SaveAll();
                    _logger.LogInformation($"Appointment ID {id} updated successfully.");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning("Update failed due to invalid model state.");
                    return View(appointment);
                }

            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment ID {Id}.", id);
                return View(appointment);
            }
                
        }
        #endregion

        #region GET: Appointments/Delete/{id}
        [Authorize(Roles = "Admin,Customer,Employee")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation($"Opening delete confirmation for appointment ID {id}.");
            var appointment = _unitOfWork.AppointmentRepository
                    .GetAll()
                    .Include(a => a.Service)  
                    .FirstOrDefault(a => a.Id == id); ;
            if (appointment == null)
            {
                _logger.LogWarning($"Delete failed: Appointment with ID {id} not found.");
                return NotFound();
            }
            return View(appointment);
        }
        #endregion

        #region POST: Appointments/Delete/{id}
        [Authorize(Roles = "Admin,Customer,Employee")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete appointment ID {id}.");
                var appointment = _unitOfWork.AppointmentRepository.GetById(id);

                if (appointment != null)
                {
                    _unitOfWork.AppointmentRepository.Delete(appointment);
                    _unitOfWork.AppointmentRepository.SaveAll();
                    _logger.LogInformation($"Successfully deleted appointment ID {id}.");
                }
                else
                {
                    _logger.LogWarning($"Delete operation: Appointment ID {id} not found.");
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting appointment ID {id}.");
                return RedirectToAction(nameof(Index));
            }

        }
        #endregion

        #region API: Get Available Time Slots
        [HttpGet]
        [Authorize(Roles = "Customer,Admin,Employee")]
        public IActionResult GetAvailableSlots(string date, int serviceId)
        {
            _logger.LogInformation("API call: GetAvailableSlots for date {Date} and service {ServiceId}",
                date, serviceId);

            // Parse the date
            if (!DateTime.TryParse(date, out DateTime selectedDate))
            {
                _logger.LogWarning("Invalid date format: {Date}", date);
                return BadRequest(new { error = "Invalid date format" });
            }

            // Get available slots from validation service
            var availableSlots = _appointmentValidationService.GetAvailableTimeSlots(selectedDate, serviceId);

            // Format slots for frontend
            var formattedSlots = availableSlots.Select(slot => new
            {
                datetime = slot.ToString("yyyy-MM-ddTHH:mm"),  // For input value
                display = slot.ToString("h:mm tt"),            // For display (9:00 AM)
                displayLong = slot.ToString("dddd, MMMM dd 'at' h:mm tt")  // Full display
            }).ToList();

            _logger.LogInformation("Returning {Count} available slots", formattedSlots.Count);

            return Json(formattedSlots);
        }
        #endregion
    }
}
