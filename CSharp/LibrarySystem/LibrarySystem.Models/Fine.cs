namespace LibrarySystem.Models;

public class Fine
{
    public int FineId { get; set; }

    public int BorrowingId { get; set; }

    public int MemberId { get; set; }

    public decimal Amount { get; set; } = 0;

    public bool IsPaid { get; set; } = false;

    public DateTime? PaidAt { get; set; }

    public Borrowing Borrowing { get; set; } = null!;

    public Member Member { get; set; } = null!;

    public Fine()
    {
        
    }
}