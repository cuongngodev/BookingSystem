using BookingSystem.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBookingSystem.Data;
using WebBookingSystem.Models;
using WebBookingSystem.Services;

namespace WebBookingSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        private readonly ApplicationDbContext _context;


        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext _context
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            this._context = _context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if(!ModelState.IsValid)
            {
                return View(registerVM);
            }
            var userExists = await _userManager.FindByEmailAsync(registerVM.Email);
            // check user exist
            if(userExists != null)
            {
                ModelState.AddModelError("", "User already exists");
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
            // create user
            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if(result.Succeeded)
            {
                user.NormalizedEmail = user.Email.ToUpper();
                user.NormalizedUserName = user.UserName.ToUpper();
                _context.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login", "Auth");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                Console.WriteLine($"Error: {error.Code} - {error.Description}");

            }

            return View(registerVM);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

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

        // GET: /Auth/Logout
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public static string GenerateUsernameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            int atIndex = email.IndexOf('@');
            if (atIndex > 0)
            {
                return email.Substring(0, atIndex);
            }

            return email;
        }

        [HttpPost]
        [Route("Auth/VerifyToken")]
        public async Task<IActionResult> VerifyToken([FromBody] string token, [FromServices] JwtService jwtService)
        {
            // Return Unauthorized if no token is provided
            if (string.IsNullOrEmpty(token))
                return Unauthorized();


            // Validate the JWT and get the user ID; returns null if invalid
            var userId = jwtService.ValidateToken(token); 

            if (userId == null)
                return Unauthorized();

            // Retrieve the user from the database
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Unauthorized();

            // Sign in via Identity cookies so the MVC views know the user is logged in
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Return success response
            return Ok(new { message = "Token valid", email = user.Email });
        }

    }
}
