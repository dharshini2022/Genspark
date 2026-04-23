using System;

namespace BusBookingSystem.Models
{
    public class SeatBlock
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public int BlockedByUserId { get; set; }
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsReleased { get; set; } = false;

        public BusSchedule Schedule { get; set; } = null!;
    }
}
