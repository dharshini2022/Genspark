namespace LibrarySystem.Models;

public class BookCopy
{
    public enum BookCopyStatus
    {
        Available = 1,
        Borrowed = 2,
        Damaged = 3,
        Unavailable = 4,
    }
    public int CopyId { get; set; }

    public int BookId { get; set; }

    public string CopyCode { get; set; } = string.Empty;

    public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;

    public Book Book { get; set; } = null!;

    public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
    public ICollection<DamageLog>? DamageLogs { get; set; }

    public BookCopy()
    {
        
    }
}