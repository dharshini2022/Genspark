using Ecommerce.Models;

namespace Ecommerce.Models.DTOs
{
    public class PlaceOrderRequest
    {
        public int CartId { get; set; }
        public int UserAddressId { get; set; }
        public string? PromoCode { get; set; }
        public string CardNumber { get; set; } = null!;
        public string ExpiryDate { get; set; } = null!;
        public string CVV { get; set; } = null!;
    }

    public class OrderSummaryDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime PlacedAt { get; set; }
        public ICollection<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
    }

    public class OrderItemDTO
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int VendorId { get; set; }
        public string VendorStoreName { get; set; } = null!;
    }

    public class ReturnRequest
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = null!;
        public ICollection<ReturnItemRequest> Items { get; set; } = new List<ReturnItemRequest>();
    }

    public class ReturnItemRequest
    {
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = null!;
    }

    public class ReturnSummaryDTO
    {
        public int Id { get; set; }
        public string? ReturnNumber { get; set; }
        public int OrderId { get; set; }
        public string? Reason { get; set; }
        public ReturnStatus Status { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public bool IsRefunded { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public ICollection<ReturnItemDTO> Items { get; set; } = new List<ReturnItemDTO>();
    }

    public class ReturnItemDTO
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal RefundAmount { get; set; }
        public ReturnItemStatus Status { get; set; }
        public ReturnItemRefundStatus? RefundStatus { get; set; }
    }

    public class AdminRevenueDTO
    {
        public decimal TotalRevenue { get; set; }
        public decimal PlatformCommissionsFromOrders { get; set; }
        public decimal PlatformCommissionsFromSettlements { get; set; }
    }
}
