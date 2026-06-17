using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IOrderService
    {
<<<<<<< HEAD
        Task<OrderResponse> CreateOrder(CreateOrderRequest request);

        Task<OrderSummaryResponse> GetOrderDetails(int orderId);
        Task<ICollection<OrderSummaryResponse>> GetMyOrders();
        Task<ICollection<OrderSummaryResponse>> GetVendorOrders();
=======
        // ─── Customer ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Phase 1 — Place Order.
        /// Validates cart items (only in-stock + active items are converted to OrderItems),
        /// applies optional discount, creates one Shipment per vendor, clears the cart.
        /// Returns orderId + total so the client can proceed to payment.
        /// </summary>
        Task<OrderResponse> CreateOrder(CreateOrderRequest request);

        Task<OrderSummaryResponse> GetOrderDetails(int orderId);
        Task<ICollection<OrderSummaryResponse>> GetMyOrders();

        // ─── Vendor ───────────────────────────────────────────────────────────────

        Task<ICollection<OrderSummaryResponse>> GetVendorOrders();

        // ─── Admin ────────────────────────────────────────────────────────────────

>>>>>>> df96b08bf7cb5e311b4c54c79c35889ebbfd6e2e
        Task<PageResponse<OrderSummaryResponse>> GetAllOrders(OrderFilterRequest query);
    }
}
