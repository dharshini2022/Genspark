using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.DTOs;
using LibraryManagementSystem.Repository;
using System.Text.RegularExpressions;

namespace LibraryManagementSystem.Service
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;

        public MemberService(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public async Task<CreateMemberResponse> AddMember(CreateMemberRequest memberRequest)
        {
            if (memberRequest == null)
            {
                throw new ArgumentNullException(nameof(memberRequest));                
            }
            if (string.IsNullOrWhiteSpace(memberRequest.FullName))
            {
                throw new ArgumentException("Member Full Name should not be empty");
            }
            if (string.IsNullOrWhiteSpace(memberRequest.Email))
            {
                throw new ArgumentException("Email Full Name should not be empty");
            }
            if (!memberRequest.Email.Contains("@") || !memberRequest.Email.Contains(".com"))
            {
                throw new ArgumentException("Email should contain @ and .com");
            }
            if (string.IsNullOrWhiteSpace(memberRequest.PhoneNumber))
            {
                throw new ArgumentException("PhoneNumber Full Name should not be empty");
            }
            if (memberRequest.PhoneNumber.Length != 10)
            {
                throw new ArgumentException("Phone number should be exactly 10 digits");                
            }
            if (!Regex.IsMatch(memberRequest.PhoneNumber, @"^\d{10}$"))
            {
                    throw new ArgumentException("Phone number should contain only digits");                
            }
            Member member = new Member()
            {
                FullName = memberRequest.FullName,
                Email = memberRequest.Email,
                PhoneNumber = memberRequest.PhoneNumber,
            };

            var result = await _memberRepository.AddMember(member);

            if (result == null)
                throw new InvalidOperationException("Unable to create member");

            return MapToResponse(result);
        }

        public async Task<CreateMemberResponse> GetMember(int memberId)
        {
            var result = await _memberRepository.GetMember(memberId);

            if (result == null)
                throw new KeyNotFoundException("Member Not Found");

            return MapToResponse(result);
        }

        public async Task<List<CreateMemberResponse>> GetMembers()
        {
            var members = await _memberRepository.GetMembers();

            return members.Select(MapToResponse).ToList();
        }

        private CreateMemberResponse MapToResponse(Member result)
        {
            return new CreateMemberResponse()
            {
                MemberId = result.MemberId,
                FullName = result.FullName,
                Email = result.Email,
                PhoneNumber = result.PhoneNumber,
                IsActive = result.IsActive,
                MembershipDate = result.MembershipDate
            };
        }
    }
}