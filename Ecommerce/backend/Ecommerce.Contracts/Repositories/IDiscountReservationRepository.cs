using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IDiscountReservationRepository : IRepository<int, DiscountReservation>
    {
        Task<DiscountReservation> Reserve(int orderId, int discountId);
        Task<DiscountReservation?> GetActiveByOrderId(int orderId);
        Task<int> ReleaseByOrderId(int orderId);
    }
}
