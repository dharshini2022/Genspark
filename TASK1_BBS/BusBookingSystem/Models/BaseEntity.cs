using System;

namespace BusBookingSystem.Models
{
    /// <summary>
    /// Abstract base class for all database-mapped entities.
    /// Implements the Inheritance OOP principle — every domain model inherits
    /// the common primary key (Id) and audit timestamp (CreatedAt) from here.
    /// This eliminates repetition across 8+ model classes.
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
