using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class OrderRepository : AbstractRepository<int, Order>, IOrderRepository
    {
        private readonly AppDbContext _dbContext;

        public OrderRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<Order> FullOrderQuery() =>
            _dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Vendor)
                .Include(o => o.Payment)
                .Include(o => o.Discount);

        public async Task<ICollection<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await FullOrderQuery()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();
        }

        public async Task<ICollection<Order>> GetOrdersByVendorIdAsync(int vendorId)
        {
            return await FullOrderQuery()
                .Where(o => o.Items.Any(oi => oi.VendorId == vendorId))
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();
        }

        public async Task<ICollection<Order>> GetOrdersByProductIdAsync(int productId)
        {
            return await FullOrderQuery()
                .Where(o => o.Items.Any(oi => oi.Variant.ProductId == productId))
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsByIdAsync(int orderId)
        {
            return await FullOrderQuery()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<ICollection<Order>> GetActiveVendorOrdersAsync(int vendorId)
        {
            var activeStatuses = new[] { OrderStatus.Confirmed, OrderStatus.Shipped };
            return await FullOrderQuery()
                .Where(o => o.Items.Any(oi => oi.VendorId == vendorId)
                         && activeStatuses.Contains(o.Status))
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();
        }
    }
}
