using System;

namespace BusBookingSystem.Models
{
    /// <summary>
    /// Represents a configurable platform setting (e.g. PlatformFeePercent, RefundPolicy).
    /// OOP — Inheritance: Inherits Id and CreatedAt from BaseEntity.
    /// UpdatedAt is a domain-specific field tracking when the setting was last changed.
    /// </summary>
    public class PlatformSetting : BaseEntity
    {
        // Id and CreatedAt inherited from BaseEntity
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int UpdatedByAdminId { get; set; }
    }
}
