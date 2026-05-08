using System;
using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    /// <summary>
    /// Represents a ticket booking made by a customer.
    /// OOP — Inheritance: Inherits Id from BaseEntity.
    /// BookedAt is kept separately (domain-specific timestamp, distinct from generic CreatedAt).
    /// </summary>
    public class Booking : BaseEntity
    {
        // Id inherited from BaseEntity
        // Note: BookedAt is a domain-specific field (when the booking was placed),
        // so it is kept alongside the inherited CreatedAt (DB insert time).
        public int CustomerId { get; set; }
        public int ScheduleId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.Column("GST")]
        public decimal Gst { get; set; }
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
