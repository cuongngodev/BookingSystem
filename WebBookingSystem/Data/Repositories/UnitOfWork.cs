using BookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;

namespace WebBookingSystem.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private ServiceRepository _serviceRepository;
        private bool disposedValue;
        private readonly ILoggerFactory _logger;

        public UnitOfWork(ApplicationDbContext context, ILoggerFactory logger)
        {
            _context = context;
            _logger = logger;
        }

        public ServiceRepository ServicesRepository
        {
            get
            {
                if (_serviceRepository == null)
                {
                    var logger = _logger.CreateLogger<ServiceRepository>();
                    _serviceRepository = new(_context, logger);
                }
                return _serviceRepository;
            }
        }

        // Save all changes in one go
        public void Save()
        {
            _context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                disposedValue = true;
            }
        }
        // Clean up resources
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
