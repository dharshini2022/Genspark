using Microsoft.EntityFrameworkCore;
using System.Linq;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Context;
using LibrarySystem.Models;

namespace LibrarySystem.DAL.Repositories
{
    public class FineRepository : AbstractRepository<int, Fine>, IFineRepository
    {
        public FineRepository(LibraryDbContext dbContext) : base(dbContext)
        {
            
        }
        public decimal GetTotalUnpaidFineByMember(int memberId)
        {
            return _dbContext.Fines.Where(f => f.MemberId == memberId && !f.IsPaid).Sum(f => f.Amount);
        }
        public List<Fine> GetFineHistoryByMemberId(int memberId)
        {
            return _dbContext.Fines.Where(f => f.MemberId == memberId).ToList();
        }
        public List<Fine> GetAllActiveFines()
        {
            return _dbContext.Fines.OrderByDescending(f => f.FineId).ToList();
        }
    }
}