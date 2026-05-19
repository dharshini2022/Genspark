using LibrarySystem.DAL.Repositories;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.Models;
using LibrarySystem.DAL.Context;

namespace LibrarySystem.BLL.Services
{
    public class AuthService
    {
        private readonly IMemberRepository _memberRepo;
        private readonly LibraryDbContext _dbContext;
        public AuthService()
        {
            _dbContext = new LibraryDbContext();
            _memberRepo = new MemberRepository(_dbContext);        
        }

        public void Register(Member member)
        {
            _memberRepo.Create(member);
        }

        public Member? Login(string email, string password)
        {
            // var admin = _adminRepository
            //     .GetAll()
            //     ?.FirstOrDefault(a => a.Email == email && a.Password == password);

            // if (admin != null)
            //     return admin;

            var member = _memberRepo.ValidateLoginCredentials(email, password);

            return member;
        }
    }
}