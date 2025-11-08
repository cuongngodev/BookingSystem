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
        private readonly ILogger<GenericRepository<T>> _logger;

        public GenericRepository(ApplicationDbContext context, ILogger<GenericRepository<T>> logger)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _logger = logger;
        }

        // Get all records from the database
        public IEnumerable<T> GetAll()
        {
            _logger.LogInformation("Fetching all records of type {EntityType} at {Time}", typeof(T).Name, DateTime.Now);
            return _dbSet.ToList();
        }

        // Get a single record by ID (supports int, string, Guid, etc.)
        public T GetById(object id)
        {
            _logger.LogInformation("Fetching {EntityType} with ID {Id} at {Time}", typeof(T).Name, id, DateTime.Now);
            return _dbSet.Find(id);
        }

        // Add a new entity
        public void Add(T entity)
        {
            _logger.LogInformation("Adding new {EntityType} at {Time}", typeof(T).Name, DateTime.Now);
            _dbSet.Add(entity);
        }

        // Update an existing entity
        public void Update(T entity)
        {
            _logger.LogInformation("Updating {EntityType} at {Time}", typeof(T).Name, DateTime.Now);
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        // Delete an entity
        public void Delete(T entity)
        {
            _logger.LogInformation("Deleting {EntityType} at {Time}", typeof(T).Name, DateTime.Now);
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _logger.LogDebug("Entity {EntityType} was detached — reattaching before delete.", typeof(T).Name);
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        // Save all changes to the database
        public void SaveAll()
        {
            try
            {
                _context.SaveChanges();
                _logger.LogInformation("Saved changes for {EntityType} at {Time}", typeof(T).Name, DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes for {EntityType} at {Time}", typeof(T).Name, DateTime.Now);
                throw;
            }
        }
    }
}
