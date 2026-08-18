using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrder(CreateOrderRequest request, string? idempotencyKey = null);
        Task<OrderSummaryResponse> GetOrderDetails(int orderId);
        Task<PageResponse<OrderSummaryResponse>> GetMyOrders(OrderFilterRequest? query = null);
        Task<PageResponse<OrderSummaryResponse>> GetVendorOrders(int? vendorId = null, OrderFilterRequest? query = null);
        Task<PageResponse<OrderSummaryResponse>> GetAllOrders(OrderFilterRequest query);
    }
}
