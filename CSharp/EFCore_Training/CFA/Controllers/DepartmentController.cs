using CFA.Models;
using CFA.Services;
using Microsoft.AspNetCore.Mvc;

namespace CFA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _service;

        public DepartmentController(IDepartmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            var departments = await _service.GetAllDepartments();
            return Ok(departments);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            var department = await _service.GetDepartmentById(id);
            if (department == null)
            {
                return NotFound($"Department with ID {id} not found.");
            }
            return Ok(department);
        }

        [HttpPost]
        public async Task<ActionResult<Department>> CreateDepartment(DepartmentDTO department)
        {
            try
            {
                var created = await _service.CreateDepartment(department);
                return CreatedAtAction(nameof(GetDepartment), new { id = created.DepartmentId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDepartment(int id, DepartmentDTO department)
        {
            try
            {
                var updated = await _service.UpdateDepartment(id, department);
                if (updated == null)
                {
                    return NotFound($"Department with ID {id} not found.");
                }
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var deleted = await _service.DeleteDepartment(id);
            if (deleted == null)
            {
                return NotFound($"Department with ID {id} not found.");
            }
            return Ok(deleted);
        }
    }
}
