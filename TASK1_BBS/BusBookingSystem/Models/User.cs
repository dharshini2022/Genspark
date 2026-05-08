using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    /// <summary>
    /// Represents a platform user (Customer, Operator, or Admin).
    /// OOP — Inheritance: Inherits Id and CreatedAt from BaseEntity
    /// instead of re-declaring them (DRY principle).
    /// </summary>
    public class User : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;

        public BusOperator? OperatorProfile { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
