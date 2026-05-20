using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.DTOs;
using LibraryManagementSystem.Repository;
using LibraryManagementSystem.Service;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controller
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _MemberService;
        public MemberController(IMemberService MemberService)
        {
            _MemberService = MemberService;
        }
        [HttpGet]
        public async Task<ActionResult<List<CreateMemberResponse>>> GetMembers()
        {
            return await _MemberService.GetMembers();
        }

        [HttpGet("{memberId:int}")]
        public async Task<ActionResult<CreateMemberResponse>> GetMember(int memberId)
        {
            try
            {
                var result = await _MemberService.GetMember(memberId);
                if(result == null)      return NotFound("No Member Found with MemberId: "+memberId);
                return Ok(result);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<CreateMemberResponse>> AddMember(CreateMemberRequest MemberRequest)
        {
            try
            {
                var result = await _MemberService.AddMember(MemberRequest);
                return CreatedAtAction(nameof(GetMember),new { memberId = result?.MemberId },result);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}