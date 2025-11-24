using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBookingSystem.Data;
using WebBookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;

namespace WebBookingSystem.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IUnitOfWork unitOfWork, ILogger<AppointmentsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region GET: Appointments (Admin)
        public IActionResult Index()
        {
            _logger.LogInformation("Fetching all appointments for admin.");

            var appointments = _unitOfWork.AppointmentRepository.GetAll();

            _logger.LogInformation("Returned {Count} appointments.", appointments.Count());
            return View(appointments);

        }
        #endregion

        #region GET: Appointments/Details/{id}
        public IActionResult Details(int? id)
        {
            _logger.LogInformation($"Fetching appointments details for ID: {id}");

            if (id == null)
            {
                _logger.LogWarning("Details request failed: ID was null.");
                return BadRequest();
            }

            var appointment = _unitOfWork.AppointmentRepository.GetById(id);
            
            if (appointment == null)
            {
                _logger.LogWarning("Appointment with ID {Id} not found.", id);
                return NotFound();
            }
            return View(appointment);
        }
        #endregion

        #region GET: Appointments/Create (From Service)
        public IActionResult Create(int serviceId)
        {
            _logger.LogInformation($"Opening create appointment page for service ID: {serviceId}");
            var appointment = new Appointment
            {
                ServiceId = serviceId,
                UserId = 1, // test user
                Status = AppointmentStatus.Pending
            };

            return View(appointment);
        }
        #endregion

        #region POST: Appointments/Create
        [HttpPost]  
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
            _logger.LogInformation($"Attempting to create new appointment for user {appointment.UserId}.");
            // only for testing
            // after user class readt, it will use userId to make an appoinment
            appointment.UserId = 1;
            appointment.Status = AppointmentStatus.Pending;
            appointment.UpdatedAt = DateTime.Now;

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
        public IActionResult Edit(int id)
        {
            _logger.LogInformation($"Fetching appointment for update (ID: {id}).");
            var appointment = _unitOfWork.AppointmentRepository.GetById(id);

            if (appointment == null)
            {
                _logger.LogWarning("Edit failed: Appointment with ID {Id} not found.", id);
                return NotFound();
            }

            return View(appointment);
        }
        #endregion

        #region POST: Appointments/Edit/5
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
                    appointment.UpdatedAt = DateTime.Now;
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
        public IActionResult Delete(int id)
        {
            _logger.LogInformation($"Opening delete confirmation for appointment ID {id}.");
            var appointment = _unitOfWork.AppointmentRepository.GetById(id);
            if (appointment == null)
            {
                _logger.LogWarning($"Delete failed: Appointment with ID {id} not found.");
                return NotFound();
            }
            return View(appointment);
        }
        #endregion

        #region POST: Appointments/Delete/{id}
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

        #region  GET: User's Appointments
        public IActionResult MyAppointments()
        {
            // only for testing,
            // after user class readt, it will use userId to make an appoinment
            int currentUserId = 1;
            _logger.LogInformation($"Fetching appointments for user ID {currentUserId}.");

            var appointments = _unitOfWork.AppointmentRepository.GetAppointmentsByUser(currentUserId);

            _logger.LogInformation($"Returned {appointments.Count()} appointments for user ID {currentUserId}.");
            return View(appointments);
        }
        #endregion
    }
}
