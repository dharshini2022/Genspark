using LibrarySystem.BLL.Interfaces;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;

namespace LibrarySystem.BLL.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepo;

        public MemberService(IMemberRepository memberRepository)
        {
            _memberRepo = memberRepository;
        }

        public Member ViewProfile(int memberId)
        {
            var member = _memberRepo.GetById(memberId);

            if(member == null)
                throw new InvalidMemberException("Member not found");

            return member;
        }

        public Member UpdateProfile(int memberId, string name, string email)
        {
            var member = _memberRepo.GetById(memberId);

            if(member == null)
                throw new InvalidMemberException("Member not found");

            member.Name = name;
            member.Email = email;

            return _memberRepo.Update(memberId, member)!;
        }

        public List<Member> ViewAllMembers()
        {
            return _memberRepo.GetAll()!;
        }

        public Member SearchMemberById(int memberId)
        {
            var member = _memberRepo.GetById(memberId);

            if(member == null)
                throw new InvalidMemberException("Member not found");

            return member;
        }

        public List<Member> SearchMemberByName(string name)
        {
            return _memberRepo.GetByName(name);
        }

        public Member SearchMemberByEmail(string email)
        {
            var member = _memberRepo.GetByEmail(email);

            if(member == null)
                throw new InvalidMemberException("Member not found");

            return member;
        }

        public bool DeactivateMember(int memberId)
        {
            var member = _memberRepo.GetById(memberId);

            if(member == null)
                throw new InvalidMemberException("Member not found");

            return _memberRepo.DeactivateMember(memberId);
        }

        public Member? UpdateMembershipType(int memberID, Member.membershipType type)
        {
            try
            {
                return _memberRepo.UpdateMember(memberID, type);
            }catch(Exception ex)
            {
                Console.WriteLine("Unable to update membership type",ex);
            }
            return null;
        }
    }
}