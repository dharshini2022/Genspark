using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/vendor/settlements")]
    [Authorize]
    public class VendorSettlementController : ControllerBase
    {
        private readonly IVendorSettlementService _vendorSettlementService;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICurrentUserService _currentUser;

        public VendorSettlementController(
            IVendorSettlementService vendorSettlementService,
            IVendorRepository vendorRepository,
            ICurrentUserService currentUser)
        {
            _vendorSettlementService = vendorSettlementService;
            _vendorRepository = vendorRepository;
            _currentUser = currentUser;
        }

        [HttpGet]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetMySettlements()
        {
            try
            {
                var vendor = await _vendorRepository.GetByUserId(_currentUser.UserId) 
                    ?? throw new KeyNotFoundException("Vendor profile not found for current user.");
                
                var settlements = await _vendorSettlementService.GetVendorSettlements(vendor.Id);
                return Ok(settlements);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("overall")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverallSettlement([FromQuery] PageRequest request)
        {
            try
            {
                var result = await _vendorSettlementService.GetOverallSettlements(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
