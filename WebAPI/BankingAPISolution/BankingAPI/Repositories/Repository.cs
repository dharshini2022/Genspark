using BankingAPI.Contexts;
using BankingAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;



namespace BankingAPI.Repositories
{
    public class Repository<K, T> : IRepository<K, T> where T : class
    {
        protected  BankingContext _context;
        public Repository(BankingContext context)
        {
            _context = context;
        }

        public async Task<T> Create(T item)
        {
            _context.Set<T>().Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<T?> Delete(K key)
        {
            var item = await Get(key);
            if (item == null)
                throw new Exception("No Such item for delete");
            _context.Set<T>().Remove(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<T?> Get(K key)
        {
            var item = await _context.Set<T>().FindAsync(key);
            return item;
        }
        
        public async Task<List<T>> GetAll()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> Update(K key, T item)
        {
            var myItem = await Get(key);
            if (myItem == null)
                throw new Exception("No such item for update");
            _context.Update(item);
            _context.SaveChanges();
            return item;
        }
    }
}