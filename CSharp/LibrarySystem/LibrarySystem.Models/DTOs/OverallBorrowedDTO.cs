namespace LibrarySystem.Models.DTOs
{
    public class OverallBorrowedBookDTO
    {
        public int BorrowingId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string BookTitle { get; set; } = string.Empty;

        public string CopyCode { get; set; } = string.Empty;

        public DateTime BorrowedAt { get; set; }

        public DateTime DueDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}