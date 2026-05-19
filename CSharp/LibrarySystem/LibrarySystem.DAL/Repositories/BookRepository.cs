using Microsoft.EntityFrameworkCore;
using System.Linq;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Context;
using LibrarySystem.Models;

namespace LibrarySystem.DAL.Repositories
{
    public class BookRepository : AbstractRepository<int,Book>, IBookRepository
    {
        public BookRepository(LibraryDbContext dbContext) : base(dbContext)
        {
        }

        public List<Book>? GetByTitle(string title)
        {
            return _dbContext.Books.Where(b => b.Title == title).ToList();
        }

        public List<Book>? GetByAuthor(string author)
        {
            return _dbContext.Books.Where(b => b.Author == author).ToList();
        }

        public List<Book>? GetByCategory(int categoryId)
        {
            return _dbContext.Books.Include(b => b.Category).Where(b => b.CategoryId == categoryId).ToList();
        }
        public List<Book> GetAvailableBooks()
        {
            return _dbContext.Books.Include(b => b.Copies).Where(b => b.Copies.Any(c =>c.Status == BookCopy.BookCopyStatus.Available)).ToList();
        }
        
        public List<object> MostBorrowedBooks(int page = 1)
        {
            int pageSize = 10;
            return _dbContext.Books.Select(b => new
                {
                    b.BookId,
                    b.Title,
                    BorrowCount = b.Copies.SelectMany(c => c.Borrowings).Count()
                })
                .OrderByDescending(b => b.BorrowCount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Cast<object>()
                .ToList();
        }
    }
}
