using LibrarySystem.DAL.Context;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.DAL.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly LibraryDbContext _dbContext;

        public DashboardRepository(LibraryDbContext context)
        {
            _dbContext = context;
        }

        public List<MostBorrowedBookDTO> GetMostBorrowedBooks(int limit)
        {
            return _dbContext.MostBorrowedBooks
                .FromSqlInterpolated($"SELECT * FROM sp_get_most_borrowed_books({limit})")
                .ToList();
        }

        public List<OverdueBookDTO> GetOverdueBooksByMember(int memberId)
        {
            return _dbContext.OverdueBooks
                .FromSqlInterpolated($"SELECT * FROM sp_get_overdue_books_by_member({memberId})")
                .ToList();
        }

        public List<OverallBorrowedBookDTO> GetOverallBorrowedBooks()
        {
            return _dbContext.OverallBorrowedBooks
                .FromSqlRaw("SELECT * FROM sp_get_overall_borrowed_books()")
                .ToList();
        }

        public List<OverdueBookDTO> GetOverallOverdueBooks()
        {
            return _dbContext.OverdueBooks
                .FromSqlRaw("SELECT * FROM sp_get_overall_overdue_books()")
                .ToList();
        }

        public List<MemberPendingFineDTO> GetMembersWithPendingFines()
        {
            return _dbContext.MemberPendingFines
                .FromSqlRaw("SELECT * FROM sp_get_members_with_pending_fines()")
                .ToList();
        }
    }

}