using LibrarySystem.Models.DTOs;

namespace LibrarySystem.DAL.Interfaces
{
    public interface IDashboardRepository
    {
        List<MostBorrowedBookDTO> GetMostBorrowedBooks(int limit);
        List<OverdueBookDTO> GetOverdueBooksByMember(int memberId);
        List<OverallBorrowedBookDTO> GetOverallBorrowedBooks();
        List<OverdueBookDTO> GetOverallOverdueBooks();
        List<MemberPendingFineDTO> GetMembersWithPendingFines();
    }
}