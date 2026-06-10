using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IOrderItemRepository : IRepository<int, OrderItem>
    {
        Task<ICollection<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
    }
}
