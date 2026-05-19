using LibrarySystem.DAL.Interfaces;
using LibrarySystem.Models;
namespace LibrarySystem.DAL.Repositories
{
    public interface IBookCopyRepository : IRepository<int, BookCopy>
    {
        public BookCopy? UpdateCopyStatus(int copyId,BookCopy.BookCopyStatus status);
        public List<BookCopy> GetAvailableCopies(int bookId);
        public List<BookCopy> GetBorrowedCopies(int bookId);
        public BookCopy? MarkAsUnavailable(int copyId);
        public BookCopy? MarkAsDamaged(int copyId);
        public List<BookCopy> GetBookCopiesByBook(int bookId);
    }
}