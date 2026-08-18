using System;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class DiscountReservationRepository : AbstractRepository<int, DiscountReservation>, IDiscountReservationRepository
    {
        private readonly AppDbContext _dbContext;

        public DiscountReservationRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DiscountReservation> Reserve(int orderId, int discountId)
        {
            var reservation = new DiscountReservation
            {
                OrderId = orderId,
                DiscountId = discountId,
                ReservedAt = DateTime.Now,
                IsReleased = false
            };
            _dbContext.DiscountReservations.Add(reservation);
            await _dbContext.SaveChangesAsync();
            return reservation;
        }

        public async Task<DiscountReservation?> GetActiveByOrderId(int orderId)
        {
            return await _dbContext.DiscountReservations
                .Include(r => r.Discount)
                .FirstOrDefaultAsync(r => r.OrderId == orderId && !r.IsReleased);
        }

        public async Task<int> ReleaseByOrderId(int orderId)
        {
            var reservations = await _dbContext.DiscountReservations
                .Where(r => r.OrderId == orderId && !r.IsReleased)
                .ToListAsync();

            foreach (var reservation in reservations)
            {
                reservation.IsReleased = true;
                reservation.ReleasedAt = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
            return reservations.Count;
        }
    }
}
