using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    /// <summary>
    /// Represents a bus registered on the platform.
    /// OOP — Inheritance: Inherits Id and CreatedAt from BaseEntity.
    /// </summary>
    public class Bus : BaseEntity
    {
        // Id and CreatedAt inherited from BaseEntity
        public int OperatorId { get; set; }
        public string BusName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string BusType { get; set; } = string.Empty; // AC, Non-AC, Sleeper
        public int? LayoutId { get; set; }
        public BusStatus Status { get; set; } = BusStatus.Pending;
        public int? ApprovedByAdminId { get; set; }
        public string? Features { get; set; } // Comma separated features
        public string? Photos { get; set; }   // Comma separated photo URLs

        public BusOperator Operator { get; set; } = null!;
        public BusLayout? Layout { get; set; }
        public ICollection<BusSchedule> Schedules { get; set; } = new List<BusSchedule>();
    }
}
