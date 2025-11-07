using BookingSystem.Data.Entities;
using WebBookingSystem.Data.Repositories;

namespace WebBookingSystem.Data.Intefaces
{
    public interface IUnitOfWork : IDisposable
    {
        ServiceRepository ServicesRepository { get; }
    }
}
