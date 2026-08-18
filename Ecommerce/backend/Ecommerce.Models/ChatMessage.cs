using System;

namespace Ecommerce.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; }
        public string Sender { get; set; } = null!; 
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public ChatSession ChatSession { get; set; } = null!;
    }
}
