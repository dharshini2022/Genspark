using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;
        private readonly IVendorService _vendorService;
        private readonly ICurrentUserService _currentUserService;

        public DiscountController(IDiscountService discountService, IVendorService vendorService, ICurrentUserService currentUserService)
        {
            _discountService = discountService;
            _vendorService = vendorService;
            _currentUserService = currentUserService;
        }

        
        [AllowAnonymous]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveDiscounts([FromQuery] PageRequest request)
        {
            try
            {
                var result = await _discountService.GetActiveDiscounts(request);
            return Ok(result);
            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
            
        }


        [Authorize(Roles = "Vendor")]
        [HttpGet("vendor")]
        public async Task<IActionResult> GetMyVendorDiscounts()
        {
            try
            {
                var vendor = await _vendorService.GetVendorByUserId(_currentUserService.UserId);
                if (vendor == null) return NotFound(new { message = "Vendor profile not found." });

                var result = await _discountService.GetVendorDiscounts(vendor.Id);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
            
        }

   


        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDiscounts()
        {
            try
            {
                var result = await _discountService.GetAllDiscounts();
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
            
        }

 
        [Authorize(Roles = "Admin")]
        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetVendorDiscountsByAdmin([FromRoute] int vendorId)
        {
            try
            {
                var result = await _discountService.GetVendorDiscounts(vendorId);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
            
        }


        [Authorize(Roles = "Admin,Vendor")]
        [HttpPost]
        public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountRequest request)
        {
            try
            {
                Console.WriteLine("Controller Start");
                var result = await _discountService.CreateDiscount(request);
                Console.WriteLine("Controlelr End");
                return CreatedAtAction(nameof(GetActiveDiscounts), new { Id = result.Id }, new
                {
                    message = "Discount created successfully.",
                    data = result
                });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
        }

        [Authorize(Roles = "Admin,Vendor")]
        [HttpPatch("deactivate/{discountCode}")]
        public async Task<IActionResult> DeactivateDiscount([FromRoute] string discountCode)
        {
            try
            {
                var result = await _discountService.DeactivateDiscount(discountCode);
                return Ok(new { message = "Discount deactivated successfully.", data = result });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
        }

        
        [Authorize(Roles = "Customer")]
        [HttpPost("evaluate")]
        public async Task<IActionResult> EvaluateCartDiscounts([FromBody] CartEvaluationRequest request)
        {
            try{
                var result = await _discountService.EvaluateCartDiscounts(request);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
            
        }
    }
}
