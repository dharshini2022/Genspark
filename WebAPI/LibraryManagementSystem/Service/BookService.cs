using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.DTOs;
using LibraryManagementSystem.Repository;



namespace LibraryManagementSystem.Repository
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }
        public async Task<CreateBookResponse?> AddBook(CreateBookRequest bookRequest)
        {
            if(bookRequest == null)
            {
                throw new ArgumentNullException("nameof(bookRequest)");                
            }    
            if (string.IsNullOrWhiteSpace(bookRequest.Title))
            {
                throw new ArgumentException("Book title should not be empty");   
            }
            if (string.IsNullOrWhiteSpace(bookRequest.Author))
            {
                throw new ArgumentException("Author name should not be empty");               
            }
            if (bookRequest.AvailableCopies < 0)
            {
                throw new ArgumentException("Available copies should be greater than or equal to 0");                
            }
            Book book = new Book()
            {
                Title = bookRequest.Title,
                Author = bookRequest.Author,
                ISBN = bookRequest.ISBN,
                PublishedYear = bookRequest.PublishedYear,
                AvailableCopies = bookRequest.AvailableCopies
            };
            var result = await _bookRepository.AddBook(book);
            if(result == null)  throw new InvalidOperationException("Unable to Add book");
            return MapToResponse(result);
        }
        public async Task<CreateBookResponse?> GetBook(int bookId)
        {
            var result = await _bookRepository.GetBook(bookId);
            if(result == null)  throw new KeyNotFoundException("Book Not Found");
            return MapToResponse(result);
        }
        public async Task<List<CreateBookResponse>> GetBooks()
        {
            List<Book> bookList = await _bookRepository.GetBooks();
            return bookList.Select(MapToResponse).ToList();
        }
        private CreateBookResponse MapToResponse(Book book)
        {
            return new CreateBookResponse()
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublishedYear = book.PublishedYear,
                AvailableCopies = book.AvailableCopies
            };
        }
    }
}