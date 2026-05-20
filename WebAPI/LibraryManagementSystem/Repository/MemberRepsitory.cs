using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repository
{
    public class MemberRepository : IMemberRepository
    {
        private readonly LibraryDbContext _dbContext;
        public MemberRepository(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Member?> AddMember(Member member)
        {
            _dbContext.Members.Add(member);
            await _dbContext.SaveChangesAsync();
            return member;
        }
        public async Task<Member?> GetMember(int MemberId)
        {
            return await _dbContext.Members.FindAsync(MemberId);
        }
        public async Task<List<Member>> GetMembers()
        {
            return await _dbContext.Members.ToListAsync();
        }
    }
}