using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repository
{
    public interface IBookRepository
    {
        public Task<Book?> AddBook(Book book);
        public Task<Book?> GetBook(int BookId);
        public Task<List<Book>> GetBooks();
    }
}