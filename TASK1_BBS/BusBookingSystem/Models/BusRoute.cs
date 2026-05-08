using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    /// <summary>
    /// Represents a city-to-city bus route.
    /// OOP — Inheritance: Inherits Id and CreatedAt from BaseEntity.
    /// </summary>
    public class BusRoute : BaseEntity
    {
        // Id and CreatedAt inherited from BaseEntity
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public int CreatedByAdminId { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<BusSchedule> Schedules { get; set; } = new List<BusSchedule>();
    }
}
