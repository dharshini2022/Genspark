using LibrarySystem.BLL.Interfaces;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.Models.DTOs;

namespace LibrarySystem.BLL.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IFineService _fineService;
        public DashboardService(
            IDashboardRepository dashboardRepository,
            IFineService fineService)
        {
            _dashboardRepository = dashboardRepository;
            _fineService = fineService;
        }

        public List<MostBorrowedBookDTO> GetMostBorrowedBooks(int limit = 10)
        {
            return _dashboardRepository.GetMostBorrowedBooks(limit);
        }

        public List<OverdueBookDTO> GetOverdueBooksByMember(int memberId)
        {
            return _dashboardRepository.GetOverdueBooksByMember(memberId);
        }

        public decimal GetPendingFine(int memberId)
        {
            return _fineService.GetTotalUnpaidFineByMember(memberId);
        }

        public List<OverallBorrowedBookDTO> GetOverallBorrowedBooks()
        {
            return _dashboardRepository.GetOverallBorrowedBooks();
        }

        public List<OverdueBookDTO> GetOverallOverdueBooks()
        {
            return _dashboardRepository.GetOverallOverdueBooks();
        }

        public List<MemberPendingFineDTO> GetMembersWithPendingFines()
        {
            return _dashboardRepository.GetMembersWithPendingFines();
        }
    }
}