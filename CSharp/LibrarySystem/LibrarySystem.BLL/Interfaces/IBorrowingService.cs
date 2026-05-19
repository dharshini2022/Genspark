using LibrarySystem.Models;

namespace LibrarySystem.BLL.Interfaces
{
    public interface IBorrowingService
    {
        public Borrowing? ReturnBook(int borrowingId);        public List<Borrowing> ViewActiveBorrows();
        public List<Borrowing> ViewBorrowHistoryByMember(int memberId);
        public Borrowing ApproveReturn(int borrowingId,Borrowing.BorrowingStatus status, string Remarks);
        public Borrowing BorrowBookByTitle(int memberId, string title);
        public Borrowing ReturnBookByTitle(int memberId, string title);


    }
}