using Microsoft.EntityFrameworkCore;
using System.Linq;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Context;
using LibrarySystem.Models;

namespace LibrarySystem.DAL.Repositories
{
    public class BookCopyRepository : AbstractRepository<int,BookCopy>, IBookCopyRepository
    {
        public BookCopyRepository(LibraryDbContext dbContext) : base(dbContext)
        {
        }

        public BookCopy? UpdateCopyStatus(int copyId,BookCopy.BookCopyStatus status)
        {
            var bookCopy = _dbContext.BookCopies.SingleOrDefault(bc => bc.CopyId == copyId);
            if(bookCopy == null)    return null;
            bookCopy.Status = status;
            _dbContext.SaveChanges();
            return bookCopy;
        }
        public List<BookCopy> GetAvailableCopies(int bookId)
        {
            return _dbContext.BookCopies.Where(bc => bc.BookId == bookId && bc.Status == BookCopy.BookCopyStatus.Available).ToList();
        }
        public List<BookCopy> GetBookCopiesByBook(int bookId)
        {
            return _dbContext.BookCopies.Where(bc => bc.BookId == bookId).ToList();
        }
        public List<BookCopy> GetBorrowedCopies(int bookId)
        {
            return _dbContext.BookCopies.Where(bc => bc.BookId == bookId && bc.Status == BookCopy.BookCopyStatus.Borrowed).ToList();
        }
        public BookCopy? MarkAsUnavailable(int copyId)
        {
            var bookCopy = _dbContext.BookCopies.Find(copyId);
            if(bookCopy == null)    return null;
            bookCopy.Status = BookCopy.BookCopyStatus.Unavailable;
            _dbContext.SaveChanges();
            return bookCopy;
        }
        public BookCopy? MarkAsDamaged(int copyId)
        {
            var bookCopy = _dbContext.BookCopies.Find(copyId);
            if(bookCopy == null)    return null;
            bookCopy.Status = BookCopy.BookCopyStatus.Damaged;
            _dbContext.SaveChanges();
            return bookCopy;
        }
    }

}