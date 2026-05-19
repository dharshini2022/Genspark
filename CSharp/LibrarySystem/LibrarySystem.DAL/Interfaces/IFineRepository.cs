using LibrarySystem.Models;
namespace LibrarySystem.DAL.Interfaces
{
    public interface IFineRepository : IRepository<int,Fine>
    {
        public decimal GetTotalUnpaidFineByMember(int memberId);
        public List<Fine> GetFineHistoryByMemberId(int memberId);
        public List<Fine> GetAllActiveFines();
    }    
}
