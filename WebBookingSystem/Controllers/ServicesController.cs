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
        public IActionResult Index()
        {
            var services = _unitOfWork.ServiceRepository.GetAll();
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
        [HttpGet] 
        public IActionResult Create()
        {
            return View();
        }
        #endregion


        #region POST: Services/Create
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
