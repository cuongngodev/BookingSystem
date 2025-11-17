using Microsoft.AspNetCore.Mvc;
using BookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;
using Microsoft.AspNetCore.Authorization;

namespace WebBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var services = _unitOfWork.ServicesRepository.GetAll();
            return View(services);
        }
        #endregion

        #region GET: Services/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(string name)
        {
            var service = _unitOfWork.ServicesRepository.GetServiceByName(name);
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

        // POST: Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Name,Duration,Price,Description")] Service service)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ServicesRepository.Add(service);
                _unitOfWork.ServicesRepository.SaveAll();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }
        #endregion

        # region GET: Services/Edit/5
        public IActionResult Edit(int id)
        {
            var service = _unitOfWork.ServicesRepository.GetById(id);
            if ( service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Services/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Name,Duration,Price,Description")] Service service)
        {
            if (id != service.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.ServicesRepository.Update(service);
                _unitOfWork.ServicesRepository.SaveAll();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }
        #endregion

        #region GET: Services/Delete/5  
        public IActionResult Delete(int id)
        {
            var service = _unitOfWork.ServicesRepository.GetById(id);
            if (id == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var service = _unitOfWork.ServicesRepository.GetById(id);
            if (service != null)
            {
                _unitOfWork.ServicesRepository.Delete(service);
                _unitOfWork.ServicesRepository.SaveAll();
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
