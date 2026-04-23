using System;
using System.Collections.Generic;

namespace BusBookingSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public BusOperator? OperatorProfile { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
