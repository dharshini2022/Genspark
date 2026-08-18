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
            var result = await _discountService.GetActiveDiscounts(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("product")]
        public async Task<IActionResult> GetDiscountOfProduct([FromQuery] int productId, [FromQuery] int categoryId, [FromQuery] int vendorId)
        {
            var result = await _discountService.GetDiscountsOfProduct(productId, categoryId, vendorId);
            return Ok(result);
        }



        [Authorize(Roles = "Vendor")]
        [HttpGet("vendor")]
        public async Task<IActionResult> GetMyVendorDiscounts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var vendor = await _vendorService.GetVendorByUserId(_currentUserService.UserId);
            if (vendor == null) return NotFound(new { message = "Vendor profile not found." });

            var result = await _discountService.GetVendorDiscounts(vendor.Id);
            var totalCount = result.Count;
            var items = result.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new PageResponse<DiscountResponse>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }

   


        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDiscounts([FromQuery]PageRequest request)
        {
            var result = await _discountService.GetAllDiscounts(request);
            return Ok(result);
        }

 
        [Authorize(Roles = "Admin")]
        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetVendorDiscountsByAdmin([FromRoute] int vendorId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _discountService.GetVendorDiscounts(vendorId);
            var totalCount = result.Count;
            var items = result.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new PageResponse<DiscountResponse>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }


        [Authorize(Roles = "Admin,Vendor")]
        [HttpPost]
        public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountRequest request)
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

        [Authorize(Roles = "Admin,Vendor")]
        [HttpPatch("deactivate/{discountCode}")]
        public async Task<IActionResult> DeactivateDiscount([FromRoute] string discountCode)
        {
            var result = await _discountService.DeactivateDiscount(discountCode);
            return Ok(new { message = "Discount deactivated successfully.", data = result });
        }

        
        [Authorize(Roles = "Customer")]
        [HttpPost("evaluate")]
        public async Task<IActionResult> EvaluateCartDiscounts([FromBody] CartEvaluationRequest request)
        {
            var result = await _discountService.EvaluateCartDiscounts(request);
            return Ok(result);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("applicable-locked")]
        public async Task<IActionResult> GetApplicableLockedDiscounts([FromBody] CartEvaluationRequest request)
        {
            var result = await _discountService.GetApplicableLockedDiscounts(request);
            return Ok(result);
        }
    }
}
