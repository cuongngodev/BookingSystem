using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebBookingSystem.Data;
using WebBookingSystem.Models;

namespace WebBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Accessed Home/Index at {Time}", DateTime.Now);
            try
            {
                if (User.IsInRole("Admin"))
                {
                    _logger.LogInformation("Redirecting Admin user to Admin Dashboard at {Time}", DateTime.Now);
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    var services = await _context.Services.ToListAsync();
                    return View(services);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading the Home page at {Time}", DateTime.Now);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public IActionResult AboutUs()
        {
            _logger.LogInformation("Accessed Home/AboutUs at {Time}", DateTime.Now);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogWarning("Error page displayed with RequestId: {RequestId} at {Time}", requestId, DateTime.Now);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
