using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class VendorSettlementRepository : AbstractRepository<int, VendorSettlement>, IVendorSettlementRepository
    {
        private readonly AppDbContext _dbContext;

        public VendorSettlementRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<VendorSettlement>> GetSettlementsByVendorIdAsync(int vendorId)
        {
            return await _dbContext.Set<VendorSettlement>()
                .Include(vs => vs.Vendor)
                .Include(vs => vs.Order)
                .Where(vs => vs.VendorId == vendorId)
                .OrderByDescending(vs => vs.SettledAt)
                .ToListAsync();
        }

        public async Task<ICollection<VendorSettlement>> GetSettlementsByOrderIdAsync(int orderId)
        {
            return await _dbContext.Set<VendorSettlement>()
                .Include(vs => vs.Vendor)
                .Where(vs => vs.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<ICollection<VendorSettlement>> GetSettlementsByStatusAsync(string status)
        {
            if (!Enum.TryParse<SettlementStatus>(status, true, out var settlementStatus))
                return new List<VendorSettlement>();

            return await _dbContext.Set<VendorSettlement>()
                .Include(vs => vs.Vendor)
                .Include(vs => vs.Order)
                .Where(vs => vs.Status == settlementStatus)
                .ToListAsync();
        }
    }
}
