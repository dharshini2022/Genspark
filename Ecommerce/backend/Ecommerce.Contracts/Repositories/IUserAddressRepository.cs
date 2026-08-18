using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IUserAddressRepository : IRepository<int, UserAddress>
    {
        Task<ICollection<UserAddress>> GetAddressByUserId(int userId);
        Task<ICollection<UserAddress>> GetAllAddressByUserId(int userId);
    }
}
