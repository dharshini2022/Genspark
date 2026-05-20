namespace LibraryManagementSystem.Models.DTOs
{
    public class CreateMemberResponse
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber {get; set;} = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime MembershipDate { get; set; } = DateTime.Now;

    }
}