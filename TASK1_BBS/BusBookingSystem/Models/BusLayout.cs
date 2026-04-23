using System;
using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    public class BusLayout
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int SeatsPerRow { get; set; }
        public bool HasUpperDeck { get; set; }
        public string LayoutJson { get; set; } = string.Empty; // JSON grid definition
        public int? CreatedByOperatorId { get; set; }
        public bool IsGlobal { get; set; } = false; // admin-created templates
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Bus> Buses { get; set; } = new List<Bus>();
    }
}
