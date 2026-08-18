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

        public VendorSettlementController(IVendorSettlementService vendorSettlementService,IVendorRepository vendorRepository,ICurrentUserService currentUser)
        {
            _vendorSettlementService = vendorSettlementService;
            _vendorRepository = vendorRepository;
            _currentUser = currentUser;
        }

        [HttpGet]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetMySettlements([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var vendor = await _vendorRepository.GetByUserId(_currentUser.UserId) 
                ?? throw new KeyNotFoundException("Vendor profile not found for current user.");
            
            var settlements = await _vendorSettlementService.GetVendorSettlements(vendor.Id);
            var totalCount = settlements.Count;
            var items = settlements.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new PageResponse<VendorSettlementDTO>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }

        [HttpGet("vendor/{vendorId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetVendorSettlementsById([FromRoute] int vendorId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var settlements = await _vendorSettlementService.GetVendorSettlements(vendorId);
            var totalCount = settlements.Count;
            var items = settlements.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new PageResponse<VendorSettlementDTO>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }

        [HttpGet("overall")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverallSettlement([FromQuery] PageRequest request)
        {
            var result = await _vendorSettlementService.GetOverallSettlements(request);
            return Ok(result);
        }
    }
}
