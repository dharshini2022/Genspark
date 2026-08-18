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

        [HttpPost("pay/{orderId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MakePayment([FromRoute] int orderId, [FromBody] MakePaymentRequest request)
        {
            var result = await _paymentService.MakePayment(orderId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("checkout-session/{orderId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateCheckoutSession([FromRoute] int orderId)
        {
            var url = await _paymentService.CreateCheckoutSessionUrl(orderId);
            return Ok(new { url });
        }

        [HttpGet("history")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyPaymentHistory()
        {
            var result = await _paymentService.GetMyPaymentHistory();
            return Ok(result);
        }

        [HttpGet("overall")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverallPaymentHistory([FromQuery] PageRequest request)
        {
            var result = await _paymentService.GetOverallPaymentHistory(request);
            return Ok(result);
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
            try
            {
                await _paymentService.HandleStripeWebhookEvent(json, signatureHeader);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Webhook Error: {ex.Message}" });
            }
        }
    }
}
