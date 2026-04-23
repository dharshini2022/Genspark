using System;
using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    public class BusRoute
    {
        public int Id { get; set; }
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public int CreatedByAdminId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public ICollection<BusSchedule> Schedules { get; set; } = new List<BusSchedule>();
    }
}
