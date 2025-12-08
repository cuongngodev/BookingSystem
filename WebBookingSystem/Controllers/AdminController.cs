using Microsoft.AspNetCore.Mvc;
using WebBookingSystem.Data.Intefaces;

namespace WebBookingSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AppointmentsController> _logger;
        public AdminController(IUnitOfWork unitOfWork, ILogger<AppointmentsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
