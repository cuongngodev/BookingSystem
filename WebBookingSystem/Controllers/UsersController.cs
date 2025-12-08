using BookingSystem.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBookingSystem.Data.Intefaces;

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

        // GET: Users (Admin Only)
        // Fetches all users
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            _logger.LogInformation("Fetching all users (Admin)");
            var users = _unitOfWork.UserRepository.GetAllUsers();
            //return Ok(users);
            return View(users);
        }

        // GET: Users/SearchByName?name=...
        // Search users by first or last name (Admin Only)
        [Authorize(Roles = "Admin")]
        public IActionResult SearchByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("SearchByName called with empty name");
                return Json(new { success = false, message = "Please enter a name." });
            }

            var users = _unitOfWork.UserRepository.GetAllUsers();
            var result = users.Where(
                user => user.LastName.ToLower().Contains(name.ToLower()) || user.FirstName.ToLower().Contains(name.ToLower()))
                .Select(user => new
                {
                    user.Id,
                    user.Email,
                    FullName = $"{user.FirstName}, {user.LastName}",
                    user.PhoneNumber
                })
                .ToList();

            if (!result.Any())
            {
                _logger.LogInformation("No users found for search: {Name}", name);
                return null;
            }

            _logger.LogInformation("Found {Count} users matching search: {Name}", result.Count, name);
            return Ok(result);
        }

        // GET: Users/Details/{id}
        [Authorize]
        public IActionResult Details(int id)
        {
            var user = _unitOfWork.UserRepository.GetUserById(id);
            if (user == null)
            {
                _logger.LogWarning("Details: User {UserId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Details: Viewing user {UserId}", id);
            return View(user);
        }

        // GET: Users/Appointments/{id}
        [Authorize]
        public IActionResult Appointments(int id)
        {
            var user = _unitOfWork.UserRepository.GetUserById(id);
            if (user == null)
            {
                _logger.LogWarning("Appointments: User {UserId} not found", id);
                return NotFound();
            }

            var appointments = _unitOfWork.AppointmentRepository.GetAppointmentsByUser(id);
            _logger.LogInformation("Appointments: Fetched {Count} appointments for user {UserId}", appointments.Count(), id);
            return View(appointments);
        }

        // GET: Users/Create
        [AllowAnonymous]
        public IActionResult Create() => View();

        // POST: Users/Create
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationUser user)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create POST failed: ModelState invalid");
                return View(user);
            }

            _unitOfWork.UserRepository.Add(user);
            _unitOfWork.UserRepository.SaveAll();

            _logger.LogInformation("Created new user with email {Email}", user.Email);
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Edit/{id}
        [Authorize]
        public IActionResult Edit(int id)
        {
            var user = _unitOfWork.UserRepository.GetUserById(id);
            if (user == null)
            {
                _logger.LogWarning("Edit GET: User {UserId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Edit GET: User {UserId} loaded", id);
            return View(user);
        }

        // POST: Users/Edit/{id}
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ApplicationUser updatedUser)
        {
            if (id != updatedUser.Id)
            {
                _logger.LogWarning("Edit POST: Mismatched IDs {Id} != {UpdatedId}", id, updatedUser.Id);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Edit POST: ModelState invalid for user {UserId}", id);
                return View(updatedUser);
            }

            _unitOfWork.UserRepository.Update(updatedUser);
            _unitOfWork.UserRepository.SaveAll();
            _logger.LogInformation("Edit POST: Updated user {UserId}", id);

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Delete/{id}
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var user = _unitOfWork.UserRepository.GetUserById(id);
            if (user == null)
            {
                _logger.LogWarning("Delete GET: User {UserId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Delete GET: User {UserId} loaded", id);
            return View(user);
        }

        // POST: Users/Delete/{id}
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
                _logger.LogInformation("DeleteConfirmed: Deleted user {UserId}", id);
            }
            else
            {
                _logger.LogWarning("DeleteConfirmed: User {UserId} not found", id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
