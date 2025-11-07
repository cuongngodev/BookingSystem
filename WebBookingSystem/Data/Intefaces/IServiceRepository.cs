using BookingSystem.Data.Entities;

namespace WebBookingSystem.Data.Intefaces
{
    public interface IServiceRepository : IGenericRepository<Service>
    {
        Service? GetServiceByName(string name);
        IEnumerable<Service> GetServicesUnderPrice(decimal maxPrice);


    }
}
