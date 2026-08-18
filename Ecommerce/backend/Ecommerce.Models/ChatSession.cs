using System;
using System.Collections.Generic;

namespace Ecommerce.Models
{
    public class ChatSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
