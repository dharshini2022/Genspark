using System.Reflection;
using LibrarySystem.Models;
namespace LibrarySystem.DAL.Interfaces
{
    public interface IMemberRepository : IRepository<int,Member>
    {
        public Member? GetByEmail(string email);
        public List<Member> GetByName(string name);
        public bool MemberExists(int memberId);
        public bool DeactivateMember(int memberId);
        public Member? UpdateMember(int memberID, Member.membershipType type);
        public Member? ValidateLoginCredentials(string email, string password);
        public Member? GetMemberWithBorrowings(int memberId);
    }
}