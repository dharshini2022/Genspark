using LibrarySystem.BLL.Interfaces;
using LibrarySystem.DAL.Context;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Repositories;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.BLL.Services
{
    public class BorrowingService : IBorrowingService
    {
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly IBookCopyRepository _bookCopyRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IFineRepository _fineRepository;
        private readonly IDamageLogRepository _damageRepository;
        private readonly LibraryDbContext _dbContext;

        public BorrowingService(IBorrowingRepository borrowingRepository, IBookCopyRepository bookCopyRepository, IMemberRepository memberRepository, IFineRepository fineRepository, IDamageLogRepository damageLogRepository , LibraryDbContext dbContext)
        {
            _borrowingRepository = borrowingRepository;
            _bookCopyRepository = bookCopyRepository;
            _memberRepository = memberRepository;
            _fineRepository = fineRepository;
            _damageRepository = damageLogRepository;
            _dbContext = dbContext;
        }
        
        public Borrowing BorrowBook(int memberId, int copyId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var member = ValidateMember(memberId);

                ValidateFineLimit(memberId);

                ValidateBorrowLimit(member);

                var copy = ValidateBookCopy(copyId);

                ValidateDuplicateBorrow(memberId, copy.BookId);

                var borrowing = CreateBorrowing(member, copyId);

                _bookCopyRepository.UpdateCopyStatus(copy.CopyId,BookCopy.BookCopyStatus.Borrowed);

                _dbContext.SaveChanges();

                transaction.Commit();

                return borrowing;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public Borrowing ReturnBook(int borrowingId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var borrowing = ValidateBorrowingForReturn(borrowingId);
                int delayedDays = CalculateDelayedDays(borrowing);

                if(delayedDays > 0)
                {
                    CreateFine(borrowing, delayedDays);
                }

                UpdateBorrowStatus(borrowing.BorrowingId,Borrowing.BorrowingStatus.Returned);

                // UpdateBookCopyAsAvailable(borrowing.Copy);

                _dbContext.SaveChanges();

                transaction.Commit();

                return borrowing;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public List<Borrowing> ViewActiveBorrows()
        {
            return _borrowingRepository
                    .GetAll()!
                    .Where(b => b.Status == Borrowing.BorrowingStatus.Active)
                    .ToList();
        }

        public Borrowing UpdateBorrowStatus(int borrowingId, Borrowing.BorrowingStatus status)
        {
            var borrowing = _borrowingRepository.GetById(borrowingId);
            if(borrowing == null)   throw new Exception("Borrowing not found");

            borrowing.Status = status;
            return _borrowingRepository.Update(borrowingId, borrowing)!;
        }

        public List<Borrowing> ViewBorrowHistoryByMember(int memberId)
        {
            return _borrowingRepository
                    .GetAll()!
                    .Where(b => b.MemberId == memberId)
                    .ToList();
        }

        public Borrowing ApproveReturn(int borrowingId, Borrowing.BorrowingStatus status, string Remarks)
        {
            var borrowing = _borrowingRepository.GetById(borrowingId);
            if(borrowing == null)   throw new Exception("Borrowing not found");

            var copy = _bookCopyRepository.GetById(borrowing.CopyId);
            if(copy == null)    throw new InvalidBookException("Book copy not found");
            borrowing.Status = status;

            if(status == Borrowing.BorrowingStatus.Returned)
            {
                copy.Status = BookCopy.BookCopyStatus.Damaged;
            }
            else if(status == Borrowing.BorrowingStatus.Damaged)
            {
                copy.Status = BookCopy.BookCopyStatus.Damaged;
                CreateDamage(borrowing,Borrowing.BorrowingStatus.Damaged, Remarks);
            }
            else if(status == Borrowing.BorrowingStatus.Lost)
            {
                copy.Status = BookCopy.BookCopyStatus.Unavailable;
                CreateDamage(borrowing,Borrowing.BorrowingStatus.Lost, Remarks);
            }

            _bookCopyRepository.Update(copy.CopyId, copy);
            return _borrowingRepository.ApproveReturnBook(borrowing, status)!;
        }
        private int GetBorrowLimit(Member.membershipType type)
        {
            return type switch
            {
                Member.membershipType.Basic => 2,
                Member.membershipType.Student => 3,
                Member.membershipType.Premium => 5,
                _ => 2
            };
        }

        private int GetBorrowDays(Member.membershipType type)
        {
            return type switch
            {
                Member.membershipType.Basic => 7,
                Member.membershipType.Student => 10,
                Member.membershipType.Premium => 15,
                _ => 7
            };
        }

        private Member ValidateMember(int memberId)
        {
            var member = _memberRepository.GetById(memberId);
            if(member == null)  throw new InvalidMemberException("Member not found");
            if(!member.IsActive)    throw new InvalidMemberException("Membership inactive");
            return member;
        }

        private void ValidateFineLimit(int memberId)
        {
            decimal unpaidFine =_fineRepository.GetTotalUnpaidFineByMember(memberId);
            if(unpaidFine > 500)    throw new FineLimitExceededException($"Unpaid fine exceeds ₹500. Total Unpaid Fine :{unpaidFine}");
        }

        private void ValidateBorrowLimit(Member member)
        {
            int activeBorrowings = _borrowingRepository.GetActiveBorrowings(member.MemberId).Count;
            int limit = GetBorrowLimit(member.MembershipType);
            if(activeBorrowings >= limit)   throw new BorrowingLimitExceededException("Borrow limit reached");
        }

        private BookCopy ValidateBookCopy(int copyId)
        {
            var copy = _bookCopyRepository.GetById(copyId);
            if(copy == null)    throw new InvalidBookException("Book copy not found");
            if(copy.Status != BookCopy.BookCopyStatus.Available)    throw new Exception("Book copy unavailable");
            return copy;
        }

        private void ValidateDuplicateBorrow(int memberId,int bookId)
        {
            bool alreadyBorrowed =_borrowingRepository.HasActiveBorrowedSameBook(memberId,bookId);
            if(alreadyBorrowed)     throw new DuplicateActiveBorrowingException("Member already borrowed this book");
        }

        private Borrowing CreateBorrowing(Member member,int copyId)
        {
            var borrowing = new Borrowing()
            {
                MemberId = member.MemberId,
                CopyId = copyId,
                // BorrowedAt = DateTime.Now,
                // DueDate = DateTime.Now.AddDays(GetBorrowDays(member.MembershipType)),
                BorrowedAt = DateTime.Now.AddDays(-2),
                DueDate = DateTime.Now.AddDays(-1),
                Status = Borrowing.BorrowingStatus.Active
            };
            return _borrowingRepository.Create(borrowing)!;
        }

        private Borrowing ValidateBorrowingForReturn(int borrowingId)
        {
            var borrowing = _dbContext.Borrowings
                .Include(b => b.Copy)
                .FirstOrDefault(b =>
                    b.BorrowingId == borrowingId);

            if(borrowing == null)   throw new Exception("Borrowing not found");
            if(borrowing.ReturnedAt != null)    throw new Exception("Book already returned");

            return borrowing;
        }

        private int CalculateDelayedDays(Borrowing borrowing)
        {
            int delayedDays = (DateTime.Now.Date - borrowing.DueDate.Date).Days;
            return delayedDays > 0 ? delayedDays : 0;
        }

        private void CreateFine(Borrowing borrowing, int delayedDays)
        {
            decimal fineAmount = delayedDays * 10;
            var fine = new Fine
            {
                BorrowingId = borrowing.BorrowingId,
                MemberId = borrowing.MemberId,
                Amount = fineAmount,
                IsPaid = false
            };
            _fineRepository.Create(fine);
        }

        private void CreateDamage(Borrowing borrowing, Borrowing.BorrowingStatus damageStatus, string Remarks)
        {
            var damage = _damageRepository.Create(new DamageLog()
            {
                MemberId = borrowing.MemberId,
                CopyId = borrowing.CopyId,
                Description = Remarks,
                DateOfEntry = DateTime.Now,
            });
            if(damageStatus == Borrowing.BorrowingStatus.Lost)
            {
                damage?.status = DamageLog.DamageStatus.Lost;
            }
            else
            {
                damage?.status = DamageLog.DamageStatus.Damage;
            }
        }
    }

}