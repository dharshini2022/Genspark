using LibrarySystem.Models.DTOs;

namespace LibrarySystem.BLL.Interfaces
{
    public interface IDashboardService
    {
        List<MostBorrowedBookDTO> GetMostBorrowedBooks(int limit = 10);

        List<OverdueBookDTO> GetOverdueBooksByMember(int memberId);

        decimal GetPendingFine(int memberId);

        List<OverallBorrowedBookDTO> GetOverallBorrowedBooks();
        List<OverdueBookDTO> GetOverallOverdueBooks();
        List<MemberPendingFineDTO> GetMembersWithPendingFines();
    }
}