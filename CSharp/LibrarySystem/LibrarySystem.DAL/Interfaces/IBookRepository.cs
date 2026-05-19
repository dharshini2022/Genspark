using LibrarySystem.Models;
namespace LibrarySystem.DAL.Interfaces
{
    public interface IBookRepository : IRepository<int, Book>
    {
        List<Book>? GetByTitle(string title);
        List<Book>? GetByAuthor(string author);
        List<Book>? GetByCategory(int categoryId);
        List<Book> GetAvailableBooks();
        // List<Book> MostBorrowedBooks();
    }
}