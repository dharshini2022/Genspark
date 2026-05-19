using Microsoft.EntityFrameworkCore;
using System.Linq;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Context;
using LibrarySystem.Models;

namespace LibrarySystem.DAL.Repositories
{
    public abstract class AbstractRepository<K,T> : IRepository<K,T> where T : class
    {
        protected readonly LibraryDbContext _dbContext;
        public AbstractRepository(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public virtual T Create(T item)
        {
            _dbContext.Set<T>().Add(item);
            _dbContext.SaveChanges();
            return item;
        }
        
        public virtual T? GetById(K key)
        {
            return _dbContext.Set<T>().Find(key);
        }

        public virtual List<T>? GetAll()
        {
            return _dbContext.Set<T>().ToList();
        }

        public virtual T? Update(K key, T item)
        {
            T? existingItem = _dbContext.Set<T>().Find(key);
            if(existingItem == null)    return null;
            _dbContext.Entry(existingItem).CurrentValues.SetValues(item);
            _dbContext.SaveChanges();
            return existingItem;
        }
    }
}