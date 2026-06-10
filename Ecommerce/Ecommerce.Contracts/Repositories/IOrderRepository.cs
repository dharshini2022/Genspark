using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IOrderRepository : IRepository<int, Order>
    {
        Task<ICollection<Order>> GetOrdersByUserIdAsync(int userId);
        Task<ICollection<Order>> GetOrdersByVendorIdAsync(int vendorId);
        Task<ICollection<Order>> GetOrdersByProductIdAsync(int productId);
        Task<Order?> GetOrderWithDetailsByIdAsync(int orderId);
        Task<ICollection<Order>> GetActiveVendorOrdersAsync(int vendorId);
    }
}
