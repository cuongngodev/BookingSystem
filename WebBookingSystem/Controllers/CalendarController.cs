using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBookingSystem.Data.Intefaces;

namespace WebBookingSystem.Controllers
{
    public class CalendarController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CalendarController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //GET: /Calendar
        /// <summary>
        /// Returns the default view for the Index page.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> that renders the Index view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetEvents()
        {
            // Return strongly-typed DateTime values and avoid formatting inside the EF projection.
            var events = _unitOfWork.AppointmentRepository
                    .GetAll()
                    .Include(a => a.Service)
                    .AsNoTracking()
                    .Select(a => new
                    {
                        id = a.Id,
                        title = a.Service != null ? a.Service.Name : "Unknown",
                        start = a.AppointmentTime,
                        end = a.Service != null ? a.AppointmentTime.AddMinutes(a.Service.Duration) : a.AppointmentTime,
                        allDay = false
                    })
                    .ToList();

            return Json(events);
        }
    }
}
