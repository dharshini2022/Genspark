using System;
using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ScheduleId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAt { get; set; }
        public decimal? RefundAmount { get; set; }
        public string? CancellationReason { get; set; }

        public User Customer { get; set; } = null!;
        public BusSchedule Schedule { get; set; } = null!;
        public ICollection<BookingPassenger> Passengers { get; set; } = new List<BookingPassenger>();
        public Payment? Payment { get; set; }
    }
}
