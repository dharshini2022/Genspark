using System;
using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    /// <summary>
    /// Represents a scheduled trip for a bus on a specific route.
    /// OOP — Inheritance: Inherits Id and CreatedAt from BaseEntity.
    /// </summary>
    public class BusSchedule : BaseEntity
    {
        // Id and CreatedAt inherited from BaseEntity
        public int BusId { get; set; }
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal PricePerSeat { get; set; }
        public bool IsCancelled { get; set; } = false;

        public Bus Bus { get; set; } = null!;
        public BusRoute Route { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<SeatBlock> SeatBlocks { get; set; } = new List<SeatBlock>();
    }
}
