namespace Ecommerce.Models.DTOs
{
    public class CartResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public string? DiscountCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool IsDiscountExpired { get; set; }
    }
}
