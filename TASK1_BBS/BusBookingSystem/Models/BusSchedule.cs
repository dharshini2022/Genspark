using System;
using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    public class BusSchedule
    {
        public int Id { get; set; }
        public int BusId { get; set; }
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal PricePerSeat { get; set; }
        public bool IsCancelled { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Bus Bus { get; set; } = null!;
        public BusRoute Route { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<SeatBlock> SeatBlocks { get; set; } = new List<SeatBlock>();
    }
}
