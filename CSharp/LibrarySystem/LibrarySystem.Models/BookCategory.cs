namespace LibrarySystem.Models;

public class BookCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public ICollection<Book> Books { get; set; } = new List<Book>();

    public BookCategory()
    {
        
    }
}