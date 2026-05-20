using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.DTOs;
using LibraryManagementSystem.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }
        [HttpGet]
        public async Task<ActionResult<List<CreateBookResponse>>> GetBooks()
        {
            return await _bookService.GetBooks();
        }

        [HttpGet("{bookId:int}")]
        public async Task<ActionResult<CreateBookResponse>> GetBook(int bookId)
        {
            try
            {
                var result = await _bookService.GetBook(bookId);
                if(result == null)      return NotFound("No Book Found with BookId: "+bookId);
                return Ok(result);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<CreateBookResponse>> AddBook(CreateBookRequest bookRequest)
        {
            try
            {
                var result = await _bookService.AddBook(bookRequest);
                return CreatedAtAction(nameof(GetBook), new { bookId = result?.BookId }, result);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}