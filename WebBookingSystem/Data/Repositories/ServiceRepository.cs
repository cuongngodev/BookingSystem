using BookingSystem.Data.Entities;
using WebBookingSystem.Data.Intefaces;

namespace WebBookingSystem.Data.Repositories
{
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        public ServiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IEnumerable<Service> GetServicesUnderPrice(decimal maxPrice)
        {
            return _dbSet.Where(s => s.Price < maxPrice).ToList();
        }
        public Service? GetServiceByName(string name)
        {
            return _dbSet.FirstOrDefault(s => s.Name == name);
        }
    }
}
