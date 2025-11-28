using Microsoft.AspNetCore.Mvc;
using BookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;
using Microsoft.AspNetCore.Authorization;

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
        public IActionResult Index(string sortOrder)
        {
            // Keep current sorting info to toggle ASC/DESC links in the view
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["DurationSortParm"] = sortOrder == "duration" ? "duration_desc" : "duration";

            // Fetch sorted services from repository
            var services = _unitOfWork.ServiceRepository.GetAll(sortOrder);

            return View(services);
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
