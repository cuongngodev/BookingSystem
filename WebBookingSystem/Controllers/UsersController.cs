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
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(ILogger<UsersController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves a list of all users matching the provided name. Accessible only to users with the Admin role, used when create a new appointment
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the collection of users.</returns>
        #region GET: Users (Admin Only)
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var users = _unitOfWork.UserRepository.GetAllUsers();
            return Ok(users);
        }
        #endregion
        #region GET: Search User by Name (Admin Only)
        [Authorize(Roles = "Admin")]
        public IActionResult SearchByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Please enter a name." });

            var users = _unitOfWork.UserRepository.GetAllUsers();

            var result = users.Where(
                user => user.LastName.ToLower().Contains(name) || user.FirstName.ToLower().Contains(name))
                .Select(user => new
                {
                    user.Id,
                    user.Email,
                    FullName = $"{user.FirstName}, {user.LastName}",
                    user.PhoneNumber
                })
                .ToList();
            // if no user found
            if (!result.Any()) return null;

            return Ok(result);
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
                _unitOfWork.UserRepository.SaveAll();
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
                _unitOfWork.UserRepository.SaveAll();
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
                _unitOfWork.UserRepository.SaveAll();
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
