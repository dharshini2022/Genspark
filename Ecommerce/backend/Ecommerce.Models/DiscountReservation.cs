using System;

namespace Ecommerce.Models
{
    public class DiscountReservation
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int DiscountId { get; set; }
        public DateTime ReservedAt { get; set; } = DateTime.Now;
        public bool IsReleased { get; set; } = false;
        public DateTime? ReleasedAt { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public Discount Discount { get; set; } = null!;
    }
}
