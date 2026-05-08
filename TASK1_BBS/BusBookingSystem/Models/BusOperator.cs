using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    /// <summary>
    /// Represents a registered bus operator company.
    /// OOP — Inheritance: Inherits Id and CreatedAt from BaseEntity.
    /// </summary>
    public class BusOperator : BaseEntity
    {
        // Id and CreatedAt inherited from BaseEntity
        public int UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public int? ApprovedByAdminId { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<OperatorOffice> Offices { get; set; } = new List<OperatorOffice>();
        public ICollection<Bus> Buses { get; set; } = new List<Bus>();
    }
}
