using LibrarySystem.Models;
namespace LibrarySystem.BLL.Interfaces
{
    public interface IDamageLogService
    {
        public DamageLog? ReportDamage(int borrowingId, Borrowing.BorrowingStatus status, string remarks);
        public List<DamageLog> GetByMember(int memberId);
        public List<DamageLog> GetByCopy(int copyId);
        public List<DamageLog> GetAllDamageLogs();
    }
}