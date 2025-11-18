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

        public AppointmentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region GET: Appointments
        public IActionResult Index()
        {
            var appointments = _unitOfWork.AppointmentRepository.GetAll();
            return View(appointments);
        }
        #endregion

        #region GET: Appointments/Details/5
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

        #region GET: Appointments/Create
        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost]  // POST: Appointments/Create
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
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
        public IActionResult Edit(int id)
        {
            var appointment = _unitOfWork.AppointmentRepository.GetById(id);

            if (appointment == null)
            {
                return NotFound();
            }
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.AppointmentRepository.Update(appointment);
                _unitOfWork.AppointmentRepository.SaveAll();
                return RedirectToAction(nameof(Index));
            }
            return View(appointment);
        }
        #endregion

        #region GET: Appointments/Delete/5
        public IActionResult Delete(int id)
        {
            var appointment = _unitOfWork.AppointmentRepository.GetById(id);
            if (appointment == null)
            {
                return NotFound();
            }
            return View(appointment);
        }

        // POST: Appointments/Delete/5
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
    }
}
