using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
        {
            var result = await _orderService.CreateOrder(request, idempotencyKey);
            return Ok(new { message = result.Message, data = result });
        }


        [HttpGet("{orderId}")]
        [Authorize(Roles = "Customer,Admin,Vendor")]
        public async Task<IActionResult> GetOrderDetail([FromRoute] int orderId)
        {
            var order = await _orderService.GetOrderDetails(orderId);
            return Ok(order);
        }


        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrders([FromQuery] OrderFilterRequest query)
        {
            var orders = await _orderService.GetMyOrders(query);
            return Ok(orders);
        }


        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderFilterRequest query)
        {
            var orders = await _orderService.GetAllOrders(query);
            return Ok(orders);
        }

        [HttpGet("vendor-orders/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetVendorOrdersById([FromRoute] int id, [FromQuery] OrderFilterRequest query)
        {
            var orders = await _orderService.GetVendorOrders(id, query);
            return Ok(orders);
        }


        [HttpGet("vendor-orders")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetVendorOrders([FromQuery] OrderFilterRequest query)
        {
            var orders = await _orderService.GetVendorOrders(null, query);
            return Ok(orders);
        }
    }
}
