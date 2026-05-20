using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.DTOs;


namespace LibraryManagementSystem.Repository
{
    public interface IBookService
    {
        public Task<CreateBookResponse?> AddBook(CreateBookRequest CreateBookResponse);
        public Task<CreateBookResponse?> GetBook(int bookId);
        public Task<List<CreateBookResponse>> GetBooks();
    }
}