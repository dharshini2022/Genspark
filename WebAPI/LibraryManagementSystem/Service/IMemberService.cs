using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.DTOs;

namespace LibraryManagementSystem.Service
{
    public interface IMemberService
    {
        public Task<CreateMemberResponse> AddMember(CreateMemberRequest memberRequest);
        public Task<CreateMemberResponse> GetMember(int MemberId);
        public Task<List<CreateMemberResponse>> GetMembers();
    }
}