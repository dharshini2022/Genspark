namespace LibrarySystem.Models;

public class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string ISBN { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public BookCategory Category { get; set; } = null!;

    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();

    public Book()
    {
        
    }
}