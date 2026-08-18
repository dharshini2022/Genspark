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
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetCatalog([FromQuery] ProductFilterRequest query)
        {
            var result = await _productService.GetProductsCatalog(query);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Search query cannot be empty." });

            var result = await _productService.SearchProducts(q);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var product = await _productService.GetProductDetails(id);
            return Ok(product);
        }

        [Authorize(Roles = "Vendor")]
        [HttpGet("vendor")]
        public async Task<IActionResult> GetMyProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var products = await _productService.GetVendorProducts();
            var totalCount = products.Count;
            var items = products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new PageResponse<ProductResponse>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetProductsByVendorId([FromRoute] int vendorId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var products = await _productService.GetProductsByVendorId(vendorId);
            var totalCount = products.Count;
            var items = products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new PageResponse<ProductResponse>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }

        [Authorize(Roles = "Vendor")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateProduct(request);
            return CreatedAtAction(nameof(GetById), new { id = product.Id },
                new { message = "Product created successfully.", data = product });
        }

        [Authorize(Roles = "Vendor")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] UpdateProductRequest request)
        {
            var product = await _productService.UpdateProduct(id, request);
            return Ok(new { message = "Product updated.", data = product });
        }

        [HttpPatch("publish/{productId}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> PublishProduct(int productId)
        {
            var product = await _productService.PublishProduct(productId);
            return Ok(new { message = "Product published successfully.", data = product });
        }

        [Authorize(Roles = "Vendor")]
        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> ToggleProductStatus([FromRoute] int id)
        {
            var product = await _productService.ToggleProductStatus(id);
            return Ok(new { message = "Product status toggled successfully.", data = product });
        }
    }
}
