using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repository
{
    public interface IMemberRepository
    {
        public Task<Member?> AddMember(Member member);
        public Task<Member?> GetMember(int MemberId);
        public Task<List<Member>> GetMembers();
    }
}