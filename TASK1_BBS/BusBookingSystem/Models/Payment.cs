using System;

namespace BusBookingSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Dummy";
        public string TransactionId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        public Booking Booking { get; set; } = null!;
    }
}
