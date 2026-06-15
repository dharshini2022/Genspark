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
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var result = await _orderService.CreateOrder(request);
                return Ok(new { message = result.Message, data = result });
            }
            catch(UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex)      { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)                 { return BadRequest(new { message = ex.Message }); }
        }


        [HttpGet("{orderId}")]
        [Authorize(Roles = "Customer,Admin,Vendor")]
        public async Task<IActionResult> GetOrderDetail([FromRoute] int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderDetails(orderId);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)        { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }


        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var orders = await _orderService.GetMyOrders();
                return Ok(orders);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }


        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderFilterRequest query)
        {
            try
            {
                var orders = await _orderService.GetAllOrders(query);
                return Ok(orders);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }


        [HttpGet("vendor-orders")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetVendorOrders()
        {
            try
            {
                var orders = await _orderService.GetVendorOrders();
                return Ok(orders);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
