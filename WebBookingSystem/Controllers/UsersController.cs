using BookingSystem.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBookingSystem.Data.Intefaces;
using WebBookingSystem.Data.Repositories;

namespace WebBookingSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly UnitOfWork _unitOfWork;

        public UsersController(ILogger<UsersController> logger, UnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        #region GET: Users (Admin Only)
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var users = _unitOfWork.UserRepository.GetAllUsers();
            return View(users);
        }
        #endregion


        #region GET: Users/Details/{id}
        [Authorize]
        public IActionResult Details(int id)
        {
            var user = _unitOfWork.UserRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        #endregion


        #region GET: Users/Appointments/{id}
        [Authorize]
        public IActionResult Appointments(int id)
        {
            var user = _unitOfWork.UserRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            var appointments = _unitOfWork.AppointmentRepository.GetAppointmentsByUser(id);
            return View(appointments);
        }
        #endregion


        #region GET: Users/Create
        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }
        #endregion


        #region POST: Users/Create
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.UserRepository.Add(user);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }
        #endregion


        #region GET: Users/Edit/{id}
        [Authorize]
        public IActionResult Edit(int id)
        {
            var user = _unitOfWork.UserRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        #endregion


        #region POST: Users/Edit/{id}
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ApplicationUser updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.UserRepository.Update(updatedUser);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(updatedUser);
        }
        #endregion


        #region GET: Users/Delete/{id}
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var user = _unitOfWork.UserRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        #endregion


        #region POST: Users/Delete/{id}
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _unitOfWork.UserRepository.GetUserById(id);
            if (user != null)
            {
                _unitOfWork.UserRepository.Delete(user);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
