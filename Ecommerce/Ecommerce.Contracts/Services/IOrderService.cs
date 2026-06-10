// using Ecommerce.Models.DTOs;
// using Ecommerce.Models;

// namespace Ecommerce.Contracts.Services
// {
//     public interface IOrderService
//     {
//         // Customer
//         Task<OrderSummaryDTO> PlaceOrder(int userId, PlaceOrderRequest request);
//         Task<OrderSummaryDTO> GetOrderDetails(int orderId, int userId);
//         Task<ICollection<OrderSummaryDTO>> GetUserOrderHistory(int userId);
//         Task<OrderSummaryDTO> CancelOrder(int userId, int orderId);
//         Task<OrderSummaryDTO> UpdateOrderAddress(int userId, int orderId, int newAddressId); // allowed if status is Confirmed

//         // Vendor
//         Task<ICollection<OrderSummaryDTO>> GetVendorOrderHistory(int vendorId);
//         Task<ICollection<OrderSummaryDTO>> GetVendorOrdersByProduct(int vendorId, int productId);
//         Task<ICollection<OrderSummaryDTO>> GetVendorActiveOrders(int vendorId);

//         // Admin
//         Task<ICollection<OrderSummaryDTO>> GetAllOrders();
//         Task<ICollection<OrderSummaryDTO>> GetOrdersByVendor(int vendorId);
//         Task<ICollection<OrderSummaryDTO>> GetOrdersByProduct(int productId);
        
//         // Financials & Settlements
//         Task<AdminRevenueDTO> GetAdminRevenue();
//         Task<ICollection<VendorSettlement>> GetVendorSettlements(int vendorId, string? statusFilter);
//     }
// }
