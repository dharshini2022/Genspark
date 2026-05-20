using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repository
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryDbContext _dbContext;
        public BookRepository(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Book?> AddBook(Book book)
        {
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();
            return book;
        }
        public async Task<Book?> GetBook(int BookId)
        {
            return await _dbContext.Books.FindAsync(BookId);
        }
        public async Task<List<Book>> GetBooks()
        {
            return await _dbContext.Books.ToListAsync();
        }
    }
}