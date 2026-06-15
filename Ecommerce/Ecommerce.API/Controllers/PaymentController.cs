using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // ─── Customer: Make Payment ───────────────────────────────────────────

        /// <summary>
        /// Step 2 — Pay for a placed order.
        /// Provide a Stripe test PaymentMethod ID (e.g. "pm_card_visa" for success,
        /// "pm_card_chargeDeclined" for failure).
        /// On success: order confirmed, shipments initiated, vendor settlements created.
        /// Shipments are automatically marked Delivered after 2 hours.
        /// </summary>
        [HttpPost("pay/{orderId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MakePayment([FromRoute] int orderId, [FromBody] MakePaymentRequest request)
        {
            try
            {
                var result = await _paymentService.MakePayment(orderId, request);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("history")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyPaymentHistory()
        {
            try
            {
                var result = await _paymentService.GetMyPaymentHistory();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("overall")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverallPaymentHistory([FromQuery] PageRequest request)
        {
            try
            {
                var result = await _paymentService.GetOverallPaymentHistory(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
