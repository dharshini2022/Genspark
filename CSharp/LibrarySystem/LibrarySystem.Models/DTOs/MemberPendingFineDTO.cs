namespace LibrarySystem.Models.DTOs
{
    public class MemberPendingFineDTO
    {
        public int MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public decimal PendingFine { get; set; }
    }
}