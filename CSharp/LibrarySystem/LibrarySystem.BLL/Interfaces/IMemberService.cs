using LibrarySystem.Models;

namespace LibrarySystem.BLL.Interfaces
{
    public interface IMemberService
    {
        public Member ViewProfile(int memberId);
        public Member UpdateProfile(int memberId, string name, string email);
        public List<Member> ViewAllMembers();
        public Member SearchMemberById(int memberId);
        public List<Member> SearchMemberByName(string name);
        public Member SearchMemberByEmail(string email);
        public bool DeactivateMember(int memberId);
        public Member? UpdateMembershipType(int memberID, Member.membershipType type);
    }
}