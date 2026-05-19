using LibrarySystem.BLL.Interfaces;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Repositories;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;

namespace LibrarySystem.BLL.Services
{
    public class DamageLogService : IDamageLogService
    {
        private readonly IDamageLogRepository _damageLogRepository;
        private readonly IBorrowingRepository _borrowingRepository;
        public DamageLogService(IDamageLogRepository damageLogRepository, IBorrowingRepository borrowingRepository)
        {
            _damageLogRepository = damageLogRepository;
            _borrowingRepository = borrowingRepository;
        }
        public DamageLog? ReportDamage(int borrowingId, Borrowing.BorrowingStatus status,string remarks)
        {
            var borrowing = _borrowingRepository.GetById(borrowingId);
            if(borrowing == null)   throw new Exception("Borrowing Not Found");
            var damage = _damageLogRepository.Create(new DamageLog()
            {
                MemberId = borrowing.MemberId,
                CopyId = borrowing.CopyId,
                Description = remarks,
                DateOfEntry = DateTime.Now,
            });
            if(status == Borrowing.BorrowingStatus.Lost)
            {
                damage?.status = DamageLog.DamageStatus.Lost;
            }
            else
            {
                damage?.status = DamageLog.DamageStatus.Lost;
            }
            return damage;
        }
        public List<DamageLog> GetByMember(int memberId)
        {
            return _damageLogRepository.GetAll()!.Where(d => d.MemberId == memberId).ToList();
        }
        public List<DamageLog> GetByCopy(int copyId)
        {
            return _damageLogRepository.GetAll()!.Where(d => d.CopyId == copyId).ToList();
        }
        public List<DamageLog> GetAllDamageLogs()
        {
            return _damageLogRepository.GetAll()!;
        }
    }
}