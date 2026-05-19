using LibrarySystem.BLL.Interfaces;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Repositories;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;

namespace LibrarySystem.BLL.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBookCopyRepository _bookCopyRepository;

        public BookService(IBookRepository bookRepository, IBookCopyRepository bookCopyRepository)
        {
            _bookRepository = bookRepository;
            _bookCopyRepository = bookCopyRepository;
        }
        public List<Book> ViewAvailableBooks()
        {
            return _bookRepository.GetAvailableBooks();
        }

        public List<Book> SearchBookByTitle(string title)
        {
            return _bookRepository.GetByTitle(title)!;
        }

        public List<Book> SearchBookByAuthor(string author)
        {
            return _bookRepository.GetByAuthor(author)!;
        }

        public List<Book> FilterBooksByCategory(int categoryId)
        {
            return _bookRepository.GetByCategory(categoryId)!;
        }


        public Book? AddBook(Book book)
        {
            try
            {
                return _bookRepository.Create(book);
            }catch(Exception ex)
            {
                Console.WriteLine("Error at Book Service");
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public Book UpdateBook(int bookId, Book book)
        {
            var existingBook = _bookRepository.GetById(bookId);

            if(existingBook == null)
                throw new InvalidBookException("Book not found");

            return _bookRepository.Update(bookId, book)!;
        }

        public List<Book> ViewAllBooks()
        {
            return _bookRepository.GetAll()!;
        }

        public Book SearchByBookId(int bookId)
        {
            var book = _bookRepository.GetById(bookId);

            if(book == null)
                throw new InvalidBookException("Book not found");

            return book;
        }


        public BookCopy? AddBookCopy(int bookId, BookCopy copy)
        {
            try
            {
                var book = _bookRepository.GetById(bookId);

                if(book == null)    throw new InvalidBookException("Book not found");

                copy.BookId = bookId;
                return _bookCopyRepository.Create(copy)!;

            }catch(Exception ex)
            {
                Console.WriteLine("Error at Book Service (AddBookCopy)");
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
            return null;
        }

        public BookCopy UpdateBookCopyStatus(int copyId, BookCopy.BookCopyStatus status)
        {
            var copy = _bookCopyRepository.GetById(copyId);

            if(copy == null)
                throw new InvalidBookException("Book copy not found");

            copy.Status = status;

            return _bookCopyRepository.Update(copyId, copy)!;
        }

        public BookCopy MarkCopyStatusAsUnavailable(int copyId)
        {
            var copy = _bookCopyRepository.GetById(copyId);
            if(copy == null)
                throw new InvalidBookException("Book copy not found");

            return _bookCopyRepository.MarkAsUnavailable(copyId)!;
        }

        public BookCopy MarkCopyStatusAsDamaged(int copyId)
        {
            var copy = _bookCopyRepository.GetById(copyId);
            if(copy == null)
                throw new InvalidBookException("Book copy not found");

            return _bookCopyRepository.MarkAsDamaged(copyId)!;
        }

        public List<BookCopy> GetBookCopiesByBook(int bookId)
        {
            return _bookCopyRepository.GetBookCopiesByBook(bookId);
        }

    }
}