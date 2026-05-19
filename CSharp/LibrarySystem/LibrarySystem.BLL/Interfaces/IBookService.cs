using LibrarySystem.Models;

namespace LibrarySystem.BLL.Interfaces
{
    public interface IBookService
    {
        public List<Book> ViewAvailableBooks();
        public List<Book> SearchBookByTitle(string title);
        public List<Book> SearchBookByAuthor(string author);
        public List<Book> FilterBooksByCategory(int categoryId);
        public Book? AddBook(Book book);
        public Book UpdateBook(int bookId, Book book);
        public List<Book> ViewAllBooks();
        public Book SearchByBookId(int bookId);
        public BookCopy? AddBookCopy(int bookId, BookCopy copy);
        public BookCopy UpdateBookCopyStatus(int copyId, BookCopy.BookCopyStatus status);
        public BookCopy MarkCopyStatusAsUnavailable(int copyId);
        public BookCopy MarkCopyStatusAsDamaged(int copyId);
        public List<BookCopy> GetBookCopiesByBook(int bookId);
    }
}