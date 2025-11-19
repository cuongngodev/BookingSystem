using BookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;

namespace WebBookingSystem.Data.Repositories
{
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        private readonly ILogger<ServiceRepository> _logger;
        public ServiceRepository(ApplicationDbContext context, ILogger<ServiceRepository> logger) : base(context, logger)
        {
            _logger = logger;
        }

        public IEnumerable<Service> GetServicesUnderPrice(decimal maxPrice)
        {
            try
            {
                return _dbSet.Where(s => s.Price < maxPrice).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching services under price {MaxPrice} at {Time}", maxPrice, DateTime.Now);
                throw;
            }
        }
        public Service GetServiceByName(string name)
        {
            try
            {
                return _dbSet.FirstOrDefault(s => s.Name == name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service with name {ServiceName} at {Time}", name, DateTime.Now);
                throw;
            }
        }

        public IEnumerable<Service> GetAllServices()
        {
            return _dbSet.ToList();
        }
    }
}
