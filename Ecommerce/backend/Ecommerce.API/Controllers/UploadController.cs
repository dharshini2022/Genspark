using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;

using Microsoft.AspNetCore.Hosting;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IProductVariantService _variantService;
        private readonly IBlobStorageService _blobStorageService;

        public UploadController(IProductVariantService variantService, IBlobStorageService blobStorageService)
        {
            _variantService = variantService;
            _blobStorageService = blobStorageService;
        }

        [HttpPost("review-image")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UploadReviewImage([FromForm] IFormFile file, [FromForm] string? userName = null, [FromForm] string? productName = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            try
            {
                string fileName = $"{Guid.NewGuid()}.webp";

                using (var inputStream = file.OpenReadStream())
                using (var image = await Image.LoadAsync(inputStream))
                using (var outputMs = new MemoryStream())
                {
                    await image.SaveAsWebpAsync(outputMs);
                    outputMs.Position = 0;

                    string blobUrl = await _blobStorageService.UploadFileAsync(outputMs, fileName, "reviews", "image/webp");
                    return Ok(new { imageUrl = blobUrl });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error during upload: {ex.Message}" });
            }
        }

        [HttpPost("variant-image")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UploadProductVariantImage([FromForm] IFormFile file, [FromForm] string? productName = null, [FromForm] int? variantNo = null, [FromForm] int imageNo = 0, [FromForm] int variantId = 0)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            try
            {
                string fileName = $"{Guid.NewGuid()}.webp";

                using (var inputStream = file.OpenReadStream())
                using (var image = await Image.LoadAsync(inputStream))
                using (var outputMs = new MemoryStream())
                {
                    await image.SaveAsWebpAsync(outputMs);
                    outputMs.Position = 0;

                    string blobUrl = await _blobStorageService.UploadFileAsync(outputMs, fileName, "products", "image/webp");
                    
                    if (variantId > 0)
                    {
                        await _variantService.AddImage(variantId, new CreateProductImageRequest
                        {
                            ImageUrl = blobUrl,
                            ImageOrder = imageNo
                        });
                    }

                    return Ok(new { imageUrl = blobUrl });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error during upload: {ex.Message}" });
            }
        }

    }
}
