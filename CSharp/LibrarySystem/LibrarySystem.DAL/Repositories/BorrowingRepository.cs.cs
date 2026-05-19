using Microsoft.EntityFrameworkCore;
using System.Linq;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Context;
using LibrarySystem.Models;

namespace LibrarySystem.DAL.Repositories
{
    public class BorrowingRepository : AbstractRepository<int, Borrowing>, IBorrowingRepository
    {
        public BorrowingRepository(LibraryDbContext dbContext) : base(dbContext)
        {
        }

        public List<Borrowing> GetActiveBorrowings(int memberId)
        {
            return _dbContext.Borrowings.Where(b => b.MemberId == memberId && b.Status == Borrowing.BorrowingStatus.Active).ToList();
        }

        public bool HasActiveBorrowedSameBook(int memberId, int bookId)
        {
            return _dbContext.Borrowings
                .Include(b => b.Copy)
                .Any(b =>
                    b.MemberId == memberId &&
                    b.ReturnedAt == null &&
                    b.Copy.BookId == bookId);
        }
        public Borrowing? ReturnBook(Borrowing borrow)
        {
            borrow.Status = Borrowing.BorrowingStatus.Returned;
            _dbContext.SaveChanges();
            return borrow ?? null;
        }
        public Borrowing? ApproveReturnBook(Borrowing borrow, Borrowing.BorrowingStatus status)
        {
            borrow.Status = status;
            return new Borrowing();
        }



    }
}