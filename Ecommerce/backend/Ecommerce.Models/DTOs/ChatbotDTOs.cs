using System;
using System.Collections.Generic;

namespace Ecommerce.Models.DTOs
{
    public class ChatMessageRequest
    {
        public string Message { get; set; } = null!;
    }

    public class ChatMessageResponse
    {
        public string Reply { get; set; } = null!;
        public int SessionId { get; set; }
    }

    public class ChatMessageDTO
    {
        public int Id { get; set; }
        public string Sender { get; set; } = null!; // "User" or "AI"
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class ChatSessionDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<ChatMessageDTO> Messages { get; set; } = new();
    }

    public class GenerateSpecsRequest
    {
        public string ProductName { get; set; } = null!;
        public string ProductDescription { get; set; } = null!;
        public string SpecDescription { get; set; } = null!;
    }
}
