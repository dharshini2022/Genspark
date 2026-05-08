using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    /// <summary>
    /// Represents a bus seat layout template.
    /// OOP — Inheritance: Inherits Id and CreatedAt from BaseEntity.
    /// </summary>
    public class BusLayout : BaseEntity
    {
        // Id and CreatedAt inherited from BaseEntity
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int SeatsPerRow { get; set; }
        public bool HasUpperDeck { get; set; }
        public string LayoutJson { get; set; } = string.Empty; // JSON grid definition
        public int? CreatedByOperatorId { get; set; }
        public bool IsGlobal { get; set; } = false; // admin-created templates

        public ICollection<Bus> Buses { get; set; } = new List<Bus>();
    }
}
