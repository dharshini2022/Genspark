using Microsoft.EntityFrameworkCore;
using System.Linq;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Context;
using LibrarySystem.Models;

namespace LibrarySystem.DAL.Repositories
{
    public class MemberRepository : AbstractRepository<int, Member>, IMemberRepository
    {
        public MemberRepository(LibraryDbContext _dbContext) : base(_dbContext)
        {
        }

        public Member? GetByEmail(string email)
        {
            return _dbContext.Members.SingleOrDefault(m => m.Email == email);
        }

        public List<Member> GetByName(string name)
        {
            return _dbContext.Members.Where(m => m.Name.Contains(name)).ToList();
        }

        public bool DeactivateMember(int memberID)
        {
            var member = _dbContext.Members.Find(memberID);
            if(member == null)  return false;
            member.IsActive = false;
            _dbContext.SaveChanges();
            return true;
        }

        public Member? UpdateMember(int memberId, Member.membershipType type)
        {
            var member = _dbContext.Members.Find(memberId);
            if(member == null)  return null;
            member.MembershipType = type;
            _dbContext.SaveChanges();
            return member;
        }

        public Member? ValidateLoginCredentials(string email, string password)
        {
            var member = _dbContext.Members.FirstOrDefault(m => m.Email == email && m.IsActive && m.Password == password);
            if (member == null) return null;
            return member;
        }

        public Member? GetMemberWithBorrowings(int memberId)
        {
            return _dbContext.Members
                .Include(m => m.Borrowings)
                .FirstOrDefault(m => m.MemberId == memberId);
        }
    }
}