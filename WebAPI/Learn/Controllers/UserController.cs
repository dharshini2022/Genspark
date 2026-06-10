using Learn.Models;
using Learn.Services;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.IO;

namespace Learn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<User>> Post([FromBody] UserDTO userDTO)
        {
            var createdUser = await _userService.Add(userDTO);
            return Created("https://localhost:5248/api/User/" + createdUser.Id, createdUser);
        }

        [HttpPost("CreateBulk")]
        public async Task<ActionResult<ICollection<User>>> PostBulk([FromBody] ICollection<UserDTO> userDtos)
        {
            var createdUsers = await _userService.AddBulk(userDtos);
            return Created("https://localhost:5248/api/User/", createdUsers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(int id)
        {
            var user = await _userService.GetById(id);
            if (user == null) return NotFound($"No user found with id: {id}");
            return Ok(user);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportUsers()
        {
            // Get all users from the database as array of objects
            var users = await _userService.GetAll();
            if (users == null || users.Count == 0)  return NotFound("No users found");

            //Create a new Excel workbook
            using var workbook = new XLWorkbook();
            //Add a new worksheet to the workbook and name it "Users"
            var worksheet = workbook.Worksheets.Add("Users");

            // Row 1: Headers
            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Phone";
            worksheet.Cell(1, 5).Value = "Age";

            // Header Styling (bold)
            worksheet.Row(1).Style.Font.Bold = true;

            //applying the body data
            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cell(row, 1).Value = user.Id;
                worksheet.Cell(row, 2).Value = user.Name;
                worksheet.Cell(row, 3).Value = user.Email;
                worksheet.Cell(row, 4).Value = user.Phone;
                worksheet.Cell(row, 5).Value = user.Age;

                row++;
            }
            // Auto fit column widths to content
            worksheet.Columns().AdjustToContents();

            //Saves as Excel file and returns as a downloadable file
            // File content is stored in memory stream and then converted to byte array to be sent as response
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            // Return the file link and on clicking the link, the file will be downloaded with the name "Users.xlsx"
            // This line tells the browsed that the return type is an Excel file and the name of the file to be downloaded is "Users.xlsx"
            return File(content,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","Users.xlsx");
        }
    }
}