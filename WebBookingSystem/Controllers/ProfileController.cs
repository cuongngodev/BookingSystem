using BookingSystem.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBookingSystem.Data;
using WebBookingSystem.Models;

namespace BookingSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // GET: Profile/Index
        // Displays the logged-in user's profile along with appointments
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdString);

            _logger.LogInformation("Fetching profile for user {UserId}", userId);

            var user = await _context.Users
                .Include(u => u.Appointments)
                .ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found.", userId);
                return NotFound();
            }

            return View(user);
        }

        // GET: Profile/Update
        // Loads the profile update form pre-filled with current user info
        public IActionResult Update()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for Update GET.", userId);
                return NotFound();
            }

            var model = new ProfileUpdateViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName
            };

            _logger.LogInformation("Loaded profile update form for user {UserId}", userId);
            return View(model);
        }

        // POST: Profile/Update
        // Handles profile update submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileUpdateViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for Update POST.", userId);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid for user {UserId}", userId);
                return View(model);
            }

            // Track changes for logging
            bool firstNameChanged = user.FirstName != model.FirstName;
            bool lastNameChanged = user.LastName != model.LastName;
            bool emailChanged = user.Email != model.Email;
            bool userNameChanged = user.UserName != model.UserName;
            bool passwordChanged = !string.IsNullOrWhiteSpace(model.Password);

            // Update properties
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            // Update email if changed
            if (emailChanged)
            {
                var emailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    _logger.LogWarning("Failed to update email for user {UserId}: {Errors}", userId, string.Join(", ", emailResult.Errors.Select(e => e.Description)));
                    return View(model);
                }
                _logger.LogInformation("Updated email for user {UserId}", userId);
            }

            // Update username if changed
            if (userNameChanged)
            {
                var userNameResult = await _userManager.SetUserNameAsync(user, model.UserName);
                if (!userNameResult.Succeeded)
                {
                    foreach (var error in userNameResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    _logger.LogWarning("Failed to update username for user {UserId}: {Errors}", userId, string.Join(", ", userNameResult.Errors.Select(e => e.Description)));
                    return View(model);
                }
                _logger.LogInformation("Updated username for user {UserId}", userId);
            }

            // Update password if provided
            if (passwordChanged)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    _logger.LogWarning("Failed to update password for user {UserId}: {Errors}", userId, string.Join(", ", passwordResult.Errors.Select(e => e.Description)));
                    return View(model);
                }
                _logger.LogInformation("Updated password for user {UserId}", userId);
            }

            await _userManager.UpdateAsync(user);

            // Log all changes for auditing
            _logger.LogInformation("Profile updated for user {UserId}. Changes: FirstName={FirstNameChanged}, LastName={LastNameChanged}, Email={EmailChanged}, UserName={UserNameChanged}, Password={PasswordChanged}",
                userId, firstNameChanged, lastNameChanged, emailChanged, userNameChanged, passwordChanged);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string Password)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                _logger.LogWarning("DeleteAccount: User {UserId} not found", userId);
                return NotFound();
            }

            // Log deletion
            _logger.LogInformation("User {UserId} requested account deletion", userId);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                _logger.LogError("Failed to delete user {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return RedirectToAction("Index");
            }

            _logger.LogInformation("User {UserId} successfully deleted", userId);

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            return RedirectToAction("Index", "Home");
        }


    }
}
