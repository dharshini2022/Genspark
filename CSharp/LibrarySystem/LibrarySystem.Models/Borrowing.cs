namespace LibrarySystem.Models;

public class Borrowing
{
    public enum BorrowingStatus
    {
        Active = 1,
        Returned = 2,
        Damaged = 3,
        Lost = 4
    }
    public int BorrowingId { get; set; }

    public int MemberId { get; set; }

    public int CopyId { get; set; }

    public DateTime BorrowedAt { get; set; } = DateTime.Now;

    public DateTime DueDate { get; set; }

    public DateTime? ReturnedAt { get; set; }

    public BorrowingStatus Status { get; set; } = BorrowingStatus.Active;

    public Member Member { get; set; } = null!;

    public BookCopy Copy { get; set; } = null!;

    public Fine? FinePayment { get; set; }

    public Borrowing()
    {
        
    }
}