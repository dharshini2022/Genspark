using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class ShipmentRepository : AbstractRepository<int, Shipment>, IShipmentRepository
    {
        private readonly AppDbContext _dbContext;

        public ShipmentRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<Shipment>> GetShipmentsByVendorIdAsync(int vendorId)
        {
            return await _dbContext.Set<Shipment>()
                .Include(s => s.OrderItems)
                    .ThenInclude(oi => oi.Vendor)
                .Include(s => s.UserAddress)
                .Where(s => s.OrderItems.Any(oi => oi.VendorId == vendorId))
                .ToListAsync();
        }

        public async Task<Shipment?> GetShipmentByTrackingNumberAsync(string trackingNumber)
        {
            return await _dbContext.Set<Shipment>()
                .Include(s => s.OrderItems)
                .Include(s => s.UserAddress)
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
        }

        public async Task<ICollection<Shipment>> GetActiveShipmentsAsync()
        {
            var activeStatuses = new[] { ShipmentStatus.Pending, ShipmentStatus.Initiated };
            return await _dbContext.Set<Shipment>()
                .Include(s => s.OrderItems)
                .Include(s => s.UserAddress)
                .Where(s => activeStatuses.Contains(s.Status))
                .ToListAsync();
        }
    }
}
