using LibrarySystem.BLL.Interfaces;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Repositories;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using LibrarySystem.DAL.Context;

public class FineService : IFineService
{
    private readonly IFineRepository _fineRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly LibraryDbContext _dbContext;

    public FineService(IFineRepository fineRepository, IMemberRepository memberRepository, LibraryDbContext dbContext)
    {
        _fineRepository = fineRepository;
        _memberRepository = memberRepository;
        _dbContext = dbContext;
    }

    public List<Fine> GetFineHistoryByMemberId(int memberId)
    {
        ValidateMember(memberId);
        return _fineRepository.GetFineHistoryByMemberId(memberId);
    }

    public List<Fine> GetAllActiveFines()
    {
        return _fineRepository.GetAllActiveFines();
    }

    public decimal GetTotalUnpaidFineByMember(int memberId)
    {
        ValidateMember(memberId);
        return _fineRepository.GetTotalUnpaidFineByMember(memberId);
    }

    public Fine PayFine(int fineId)
    {
        using var transaction =_dbContext.Database.BeginTransaction();

        try
        {
            var fine = ValidateFine(fineId);

            if(fine.IsPaid) throw new Exception("Fine already paid");

            fine.IsPaid = true;
            fine.PaidAt = DateTime.Now;
            _fineRepository.Update(fineId, fine);
            _dbContext.SaveChanges();
            transaction.Commit();
            return fine;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private Member ValidateMember(int memberId)
    {
        var member = _memberRepository.GetById(memberId);
        if(member == null)  throw new InvalidMemberException( $"Member not found with id : {memberId}");
        return member;
    }

    private Fine ValidateFine(int fineId)
    {
        var fine = _fineRepository.GetById(fineId);
        if(fine == null)    throw new Exception($"Fine not found with id : {fineId}");
        return fine;
    }
}