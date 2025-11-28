using BookingSystem.Data.Entities;
using System.Net.Http.Headers;
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

        public IQueryable<Service> GetServicesUnderPrice(decimal maxPrice)
        {
            try
            {
                return _dbSet.Where(s => s.Price < maxPrice);
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
        /// <summary>
        /// Retrieves all services from the data store, sorted according to the specified order.
        /// </summary>
        /// <param name="sortOrder">A string that specifies the sort order for the results. 
        /// Valid values are "name_desc" for descending by name,
        /// "price" for ascending by price, "price_desc" for descending by price, or any other value for ascending by
        /// name.</param>
        /// <returns>An enumerable collection of services sorted as specified. The collection will be empty if no services are
        /// available.</returns>
        public IEnumerable<Service> GetAll(string sortOrder)
        {
            // all services as queryable obj
            var services = _context.Services.AsQueryable();

            // apply sorting based on the sortOrder 
            switch (sortOrder)
            {
                case "name_desc":
                    services = services.OrderByDescending(s => s.Name);
                    break;
                case "price":
                    services = services.OrderBy(s => s.Price);
                    break;
                case "price_desc":
                    services = services.OrderByDescending(s => s.Price);
                    break;
                case "duration_desc":
                    services = services.OrderByDescending(s => s.Duration);
                    break;
                default:
                    // Default sort by name ascending
                    services = services.OrderBy(s => s.Name);
                    break;
            }
            return services.ToList();

        }
    }
}
