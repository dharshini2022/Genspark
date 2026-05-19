using LibrarySystem.DAL.Interfaces;
using LibrarySystem.Models;
namespace LibrarySystem.DAL.Repositories
{
    public interface IDamageLogRepository : IRepository<int, DamageLog>
    {
        public List<DamageLog> GetByMember(int memberId);

        public List<DamageLog> GetByCopy(int copyId);
    }
}