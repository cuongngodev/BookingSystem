using BookingSystem.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBookingSystem.Data;
using WebBookingSystem.Models;
using WebBookingSystem.Services;
using Microsoft.Extensions.Logging;

namespace WebBookingSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        #region GET: /Auth/Index
        public IActionResult Index()
        {
            _logger.LogInformation("Accessed Auth index page.");
            return View();
        }
        #endregion

        #region GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("Accessed Register page.");
            return View();
        }
        #endregion

        #region POST: /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", registerVM.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration failed: Invalid model state for email {Email}", registerVM.Email);
                return View(registerVM);
            }

            var userExists = await _userManager.FindByEmailAsync(registerVM.Email);
            if (userExists != null)
            {
                _logger.LogWarning("Registration failed: User already exists with email {Email}", registerVM.Email);
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(registerVM);
            }

            var username = GenerateUsernameFromEmail(registerVM.Email);

            ApplicationUser user = new ApplicationUser()
            {
                Email = registerVM.Email,
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                UserName = username,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password); // automaatic store in db

            if (result.Succeeded)
            {
                // asign automaticll new user is customer role
                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");  // automaatic store in db

                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("User {Email} registered successfully and assigned Customer role.", user.Email);
                }
                else
                {
                    _logger.LogError("User {Email} registered but failed to assign Customer role: {Errors}",
                     user.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }

                return RedirectToAction("Login", "Auth");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Registration error for {Email}: {ErrorCode} - {ErrorDescription}", user.Email, error.Code, error.Description);
                ModelState.AddModelError("", error.Description);
            }

            return View(registerVM);
        }
        #endregion

        #region GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("Accessed Login page.");
            return View();
        }
        #endregion

        #region POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginVM loginVM, [FromServices] JwtService jwtService)
        {
            // Check if the incoming model is valid
            if (!ModelState.IsValid)
                return View(loginVM);


            // Find the user by email
            var user = await _userManager.FindByEmailAsync(loginVM.Email);


            // Check if user exists and password is correct
            if (user != null && await _userManager.CheckPasswordAsync(user, loginVM.Password))
            {
                // Sign in the user via ASP.NET Identity cookie
                var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, false);


                if (result.Succeeded)
                {
                    // Generate a JWT for client-side usage
                    var token = jwtService.GenerateToken(user);

                    // Return the token and expiration time as JSON
                    return Ok(new
                    {
                        token,
                        expiration = DateTime.UtcNow.AddMinutes(60)
                    });
                }
            }
            // If login fails, return Unauthorized with a message
            return Unauthorized(new { message = "Invalid login attempt." });
        }
        #endregion

        #region GET: /Auth/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User logging out.");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

        #region POST: /Auth/VerifyToken
        [HttpPost]
        [Route("Auth/VerifyToken")]
        public async Task<IActionResult> VerifyToken([FromBody] string token, [FromServices] JwtService jwtService)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token verification failed: No token provided.");
                return Unauthorized();
            }

            var userId = jwtService.ValidateToken(token);

            if (userId == null)
            {
                _logger.LogWarning("Token verification failed: Invalid token.");
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("Token verification failed: User not found with ID {UserId}", userId);
                return Unauthorized();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("Token verified successfully for email {Email}", user.Email);

            return Ok(new { message = "Token valid", email = user.Email });
        }
        #endregion

        #region Helper: GenerateUsernameFromEmail
        public static string GenerateUsernameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            int atIndex = email.IndexOf('@');
            return atIndex > 0 ? email.Substring(0, atIndex) : email;
        }
        #endregion
    }
}
