using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WebBookingSystem.Data.Intefaces;

using System.Threading.Tasks;



namespace WebBookingSystem.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Get all records from the database
        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        // Get a single record by ID (supports int, string, Guid, etc.)
        public T GetById(object id)
        {
            return _dbSet.Find(id);
        }

        // Add a new entity
        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        // Update an existing entity
        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        // Delete an entity
        public void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        // Save all changes to the database
        public void SaveAll()
        {
            _context.SaveChanges();
        }
    }
}
