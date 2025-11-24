using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBookingSystem.Data;
using WebBookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;

namespace WebBookingSystem.Controllers
{
    [Authorize] // all actions require a logged-in user

    public class AppointmentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region GET: Appointments (Admin)
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public IActionResult Index()
        {
            var appointments = _unitOfWork.AppointmentRepository.GetAll();
            return View(appointments);
        }
        #endregion
        [Authorize(Roles = "Customer,Admin")]
        #region GET: Appointments/Details/{id}
        public IActionResult Details(int? id)
        {
            var appointment = _unitOfWork.AppointmentRepository.GetById(id);

            if (appointment == null)
            {
                return NotFound();
            }
            return View(appointment);
        }
        #endregion

        #region GET: Appointments/Create (From Service)
        [Authorize(Roles = "Customer,Admin")]
        public IActionResult Create(int serviceId)
        {
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
            // only for testing
            // after user class readt, it will use userId to make an appoinment
            appointment.UserId = 1;
            appointment.Status = AppointmentStatus.Pending;
            appointment.UpdatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _unitOfWork.AppointmentRepository.Add(appointment);
                _unitOfWork.AppointmentRepository.SaveAll();
                return RedirectToAction(nameof(Index));
            }
            return View(appointment);
        }
        #endregion

        #region GET: Appointments/Edit/{id}
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Edit(int id)
        {
            var appointment = _unitOfWork.AppointmentRepository.GetById(id);

            if (appointment == null)
            {
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
                return BadRequest();
            }
            // only admin can change status
            // user cannot change serviceId or userId
            appointment.UpdatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                appointment.UpdatedAt = DateTime.Now;
                _unitOfWork.AppointmentRepository.Update(appointment);
                _unitOfWork.AppointmentRepository.SaveAll();
                return RedirectToAction(nameof(Index));
            }
            return View(appointment);
        }
        #endregion

        #region GET: Appointments/Delete/{id}
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var appointment = _unitOfWork.AppointmentRepository.GetById(id);
            if (appointment == null)
            {
                return NotFound();
            }
            return View(appointment);
        }
        #endregion

        #region POST: Appointments/Delete/{id}
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var appointment = _unitOfWork.AppointmentRepository.GetById(id);

            if (appointment != null)
            {
                _unitOfWork.AppointmentRepository.Delete(appointment);
                _unitOfWork.AppointmentRepository.SaveAll();
            }
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region  GET: User's Appointments
        [Authorize]
        public IActionResult MyAppointments()
        {
            // only for testing,
            // after user class readt, it will use userId to make an appoinment
            int currentUserId = 1;

            var appointments = _unitOfWork.AppointmentRepository.GetAppointmentsByUser(currentUserId);

            return View(appointments);
        }
        #endregion
    }
}
