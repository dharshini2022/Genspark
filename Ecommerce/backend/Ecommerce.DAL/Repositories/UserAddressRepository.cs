using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class UserAddressRepository : AbstractRepository<int, UserAddress>, IUserAddressRepository
    {
        private readonly AppDbContext _dbContext;

        public UserAddressRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<UserAddress>> GetAddressByUserId(int userId)
        {
            return await _dbContext.UserAddresses.Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<ICollection<UserAddress>> GetAllAddressByUserId(int userId)
        {
            return await _dbContext.UserAddresses.Where(u => u.UserId == userId).ToListAsync();
        }

    }
}
