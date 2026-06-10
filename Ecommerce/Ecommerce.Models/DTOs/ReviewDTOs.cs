namespace Ecommerce.Models.DTOs
{
    public class CreateReviewRequest
    {
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; } // 1 to 5
        public string Title { get; set; } = null!;
        public string? Body { get; set; }
        public ICollection<string> ImageUrls { get; set; } = new List<string>();
    }

    public class UpdateReviewRequest
    {
        public int Rating { get; set; }
        public string Title { get; set; } = null!;
        public string? Body { get; set; }
        public ICollection<string> ImageUrls { get; set; } = new List<string>();
    }

    public class ReviewDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int UserId { get; set; }
        public string UserFullName { get; set; } = null!;
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = null!;
        public string? Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<string> ReviewImages { get; set; } = new List<string>();
    }
}
