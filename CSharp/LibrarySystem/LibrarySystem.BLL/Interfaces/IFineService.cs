using LibrarySystem.Models;
namespace LibrarySystem.BLL.Interfaces
{
    public interface IFineService
    {
        public List<Fine> GetFineHistoryByMemberId(int memberId);
        public List<Fine> GetAllActiveFines();
        public decimal GetTotalUnpaidFineByMember(int memberId);
        public Fine PayFine(int fineId);
    }
}