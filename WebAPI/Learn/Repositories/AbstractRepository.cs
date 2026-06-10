using Learn.Contexts;
using Microsoft.EntityFrameworkCore;
namespace  Learn.Repositories
{
    public class AbstractRepository<K,T> : IRepository<K,T> where T: class
    {
        private readonly AppDbContext _dbContext;
        public AbstractRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<T> Create(T item)
        {
            _dbContext.Set<T>().Add(item);
            await _dbContext.SaveChangesAsync();
            return item;
        }
        public async Task<ICollection<T>> CreateBulk(ICollection<T> items)
        {
            _dbContext.Set<T>().AddRange(items);
            await _dbContext.SaveChangesAsync();
            return items;
        }
        public async Task<T?> GetById(K key)
        {
            return await _dbContext.Set<T>().FindAsync(key);
        }
        public async Task<ICollection<T>> GetAll()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }
        
    }
}