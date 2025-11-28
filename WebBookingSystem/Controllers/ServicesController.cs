using Microsoft.AspNetCore.Mvc;
using BookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;
using Microsoft.AspNetCore.Authorization;
using NuGet.Protocol.Core.Types;
using WebBookingSystem.Models;

namespace WebBookingSystem.Controllers
{
    public class ServicesController : Controller
    {       
        private readonly ILogger<ServicesController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ServicesController(ILogger<ServicesController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        #region  GET: Services
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber
            )
        {
            // Set ViewData for maintaining state in view
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["DurationSortParm"] = sortOrder == "Duration" ? "duration_desc" : "Duration";

            // Handle search string and page reset
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;

            // Get IQueryable from repository
            var services = _unitOfWork.ServiceRepository.GetAll();

            // Apply search filter
            if (!String.IsNullOrEmpty(searchString))
            {
                services = services.Where(s => s.Name.Contains(searchString)
                                            || s.Description.Contains(searchString));
            }
            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    services = services.OrderByDescending(s => s.Name);
                    break;
                case "Price":
                    services = services.OrderBy(s => s.Price);
                    break;
                case "price_desc":
                    services = services.OrderByDescending(s => s.Price);
                    break;
                case "Duration":
                    services = services.OrderBy(s => s.Duration);
                    break;
                case "duration_desc":
                    services = services.OrderByDescending(s => s.Duration);
                    break;
                default:
                    services = services.OrderBy(s => s.Name);
                    break;
            }
            int pageSize = 10;
            return View(await PaginatedList<Service>.CreateAsync(services, pageNumber ?? 1, pageSize));
        }
        #endregion

        #region GET: Services/Details/{id}
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var service = _unitOfWork.ServiceRepository.GetById(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }
        #endregion

        #region GET: Services/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        #endregion


        #region POST: Services/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Service service)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ServiceRepository.Add(service);
                _unitOfWork.ServiceRepository.SaveAll();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }
        #endregion

        #region GET: Services/Edit/{id}
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var service = _unitOfWork.ServiceRepository.GetById(id);
            if ( service == null)
            {
                return NotFound();
            }

            return View(service);
        }
        #endregion

        #region POST: Services/Edit/{id}
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Service service)
        {
            if (id != service.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.ServiceRepository.Update(service);
                _unitOfWork.ServiceRepository.SaveAll();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }
        #endregion

        #region GET: Services/Delete/{id}
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var service = _unitOfWork.ServiceRepository.GetById(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }
        #endregion

        #region POST: Services/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var service = _unitOfWork.ServiceRepository.GetById(id);
            if (service != null)
            {
                _unitOfWork.ServiceRepository.Delete(service);
                _unitOfWork.ServiceRepository.SaveAll();
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
