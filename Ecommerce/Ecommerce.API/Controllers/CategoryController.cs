using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetCategoryTree()
        {
            var tree = await _categoryService.GetCategoryTree();
            return Ok(tree);
        }

        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<IActionResult> GetFlatList()
        {
            var list = await _categoryService.GetFlatList();
            return Ok(list);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var category = await _categoryService.GetById(id);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug([FromRoute] string slug)
        {
            try
            {
                var category = await _categoryService.GetBySlug(slug);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var created = await _categoryService.Create(request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id },
                    new { message = "Category created successfully.", data = created });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UniquenessViolationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch(InvalidAncestorException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var updated = await _categoryService.Update(id, request);
                return Ok(new { message = "Category updated successfully.", data = updated });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UniquenessViolationException ex)
            {
                return Conflict(new { message = ex.Message});
            }
            catch(InvalidAncestorException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var result = await _categoryService.Delete(id);
                return Ok(new { message = "Category deleted successfully.", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
