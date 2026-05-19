using LibrarySystem.Models;
namespace LibrarySystem.DAL.Interfaces
{
    public interface IBorrowingRepository : IRepository<int,Borrowing>
    {
        public List<Borrowing> GetActiveBorrowings(int memberId);
        public Borrowing? ReturnBook(Borrowing borrow);
        public Borrowing? ApproveReturnBook(Borrowing borrow,Borrowing.BorrowingStatus status);
        public bool HasActiveBorrowedSameBook(int memberId, int bookId);

    }    
}
