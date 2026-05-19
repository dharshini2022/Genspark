using Microsoft.EntityFrameworkCore;
using System.Linq;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Context;
using LibrarySystem.Models;

namespace LibrarySystem.DAL.Repositories
{
    public class DamageLogRepository : AbstractRepository<int,DamageLog>, IDamageLogRepository
    {
        public DamageLogRepository(LibraryDbContext dbContext) : base(dbContext)
        {
        }
        public List<DamageLog> GetByMember(int memberId)
            {
                return _dbContext.DamageLogs.Where(d => d.MemberId == memberId).ToList();
            }
        public List<DamageLog> GetByCopy(int copyId)
        {
            return _dbContext.DamageLogs.Where(d => d.CopyId == copyId).ToList();
        }

    }
}