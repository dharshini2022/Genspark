using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _variantService;

        public ProductVariantController(IProductVariantService variantService)
        {
            _variantService = variantService;
        }

        [AllowAnonymous]
        [HttpGet("{variantId}")]
        public async Task<IActionResult> GetVariantById([FromRoute] int variantId)
        {
            try
            {
                var variant = await _variantService.GetVariantById(variantId);
                return Ok(variant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Vendor")]
        [HttpPost("{productId}")]
        public async Task<IActionResult> AddVariant([FromRoute] int productId, [FromBody] AddProductVariantRequest request)
        {
            try
            {
                var variant = await _variantService.AddVariant(productId, request);
                return Ok(new { message = "Variant added.", data = variant });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidProductException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Vendor")]
        [HttpPut("{variantId}")]
        public async Task<IActionResult> UpdateVariant([FromRoute] int variantId, [FromBody] UpdateProductVariantRequest request)
        {
            try
            {
                var variant = await _variantService.UpdateVariant(variantId, request);
                return Ok(new { message = "Variant updated.", data = variant });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidProductException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Vendor")]
        [HttpDelete("{variantId}")]
        public async Task<IActionResult> ArchiveVariant([FromRoute] int variantId)
        {
            try
            {
                var variant = await _variantService.ArchiveVariant(variantId);
                return Ok(new { message = "Variant archived successfully.", data = variant });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidProductException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Vendor")]
        [HttpPost("image/{variantId}")]
        public async Task<IActionResult> AddImage([FromRoute] int variantId, [FromBody] CreateProductImageRequest request)
        {
            try
            {
                var image = await _variantService.AddImage(variantId, request);
                return Ok(new { message = "Image added.", data = image });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize(Roles = "Vendor")]
        [HttpDelete("image/{imageId}")]
        public async Task<IActionResult> DeleteImage([FromRoute] int imageId)
        {
            try
            {
                await _variantService.DeleteImage(imageId);
                return Ok(new { message = "Image deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
