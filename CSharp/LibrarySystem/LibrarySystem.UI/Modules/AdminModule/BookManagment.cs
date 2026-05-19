using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;

namespace LibrarySystem.UI.Modules.AdminModule
{
    public class BookManagement
    {
        private readonly IBookService _bookService;
        private readonly IDamageLogService _damageLogService;

        public BookManagement(IBookService bookService, IDamageLogService damageLogService)
        {
            _damageLogService = damageLogService;
            _bookService = bookService;
        }

        public void Show()
        {
            Console.WriteLine("====== BOOK MANAGEMENT ======");

            Console.WriteLine("1. Add Book");
            Console.WriteLine("2. Update Book");
            Console.WriteLine("3. View All Books");
            Console.WriteLine("4. Search By Title");
            Console.WriteLine("5. Search By Author");
            Console.WriteLine("6. Search By Category");
            Console.WriteLine("7. Add Book Copy");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    AddBook();
                    break;

                case 2:
                    UpdateBook();
                    break;

                case 3:
                    ViewAllBooks();
                    break;

                case 4:
                    SearchByTitle();
                    break;

                case 5:
                    SearchByAuthor();
                    break;

                case 6:
                    SearchByCategory();
                    break;

                case 7:
                    AddBookCopy();
                    break;

                case 8:
                    UpdateBookCopyStatus();
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }
        }

        private void AddBook()
        {
            try
            {
                var book = new Book();

                Console.Write("Enter Title : ");
                book.Title = Console.ReadLine()!;

                Console.Write("Enter Author : ");
                book.Author = Console.ReadLine()!;

                Console.Write("Enter ISBN : ");
                book.ISBN = Console.ReadLine()!;

                Console.Write("Enter Category Id : ");
                book.CategoryId = Convert.ToInt32(Console.ReadLine());

                var addedBook = _bookService.AddBook(book);
                if(addedBook == null)   throw new Exception("Unable to add book");

                Console.WriteLine("Book Added Successfully");
                Console.WriteLine($"Book Id : {addedBook.BookId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UpdateBook()
        {
            try
            {
                Console.Write("Enter Book Id : ");
                int bookId = Convert.ToInt32(Console.ReadLine());

                var existingBook = _bookService.SearchByBookId(bookId);

                var updatedBook = new Book();

                updatedBook.BookId = existingBook.BookId;

                Console.Write("Enter New Title : ");
                updatedBook.Title = Console.ReadLine()!;

                Console.Write("Enter New Author : ");
                updatedBook.Author = Console.ReadLine()!;

                Console.Write("Enter New ISBN : ");
                updatedBook.ISBN = Console.ReadLine()!;

                Console.Write("Enter New Category Id : ");
                updatedBook.CategoryId = Convert.ToInt32(Console.ReadLine());

                Book book = _bookService.UpdateBook(bookId, updatedBook);

                Console.WriteLine("Book Updated Successfully");
            }
            catch (InvalidBookException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ViewAllBooks()
        {
            try
            {
                List<Book> books = _bookService.ViewAllBooks();

                if (books.Count == 0)
                {
                    Console.WriteLine("No Books Found");
                    return;
                }

                foreach (var book in books)
                {
                    DisplayBook(book);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SearchByTitle()
        {
            try
            {
                Console.Write("Enter Title : ");
                string title = Console.ReadLine()!;

                List<Book> books = _bookService.SearchBookByTitle(title);

                if (books.Count == 0)
                {
                    Console.WriteLine("No Books Found");
                    return;
                }

                foreach (var book in books)
                {
                    DisplayBook(book);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SearchByAuthor()
        {
            try
            {
                Console.Write("Enter Author : ");
                string author = Console.ReadLine()!;

                List<Book> books = _bookService.SearchBookByAuthor(author);

                if (books.Count == 0)
                {
                    Console.WriteLine("No Books Found");
                    return;
                }

                foreach (var book in books)
                {
                    DisplayBook(book);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SearchByCategory()
        {
            try
            {
                Console.Write("Enter Category Id : ");
                int categoryId = Convert.ToInt32(Console.ReadLine());

                List<Book> books = _bookService.FilterBooksByCategory(categoryId);

                if (books.Count == 0)
                {
                    Console.WriteLine("No Books Found");
                    return;
                }

                foreach (var book in books)
                {
                    DisplayBook(book);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AddBookCopy()
        {
            try
            {
                Console.Write("Enter Book Id : ");
                int bookId = Convert.ToInt32(Console.ReadLine());

                BookCopy copy = new BookCopy();
                copy.Status = BookCopy.BookCopyStatus.Available;
                Console.WriteLine("Enter Copy Code");
                copy.CopyCode = Console.ReadLine() ?? "";
                copy.BookId = bookId;

                var addedCopy = _bookService.AddBookCopy(bookId, copy);
                if(addedCopy == null)   throw new Exception("Unable to add book copy");

                Console.WriteLine("Book Copy Added Successfully");
                Console.WriteLine($"Copy Id : {addedCopy.CopyId}");
            }
            catch (InvalidBookException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UpdateBookCopyStatus()
        {
            try
            {
                Console.Write("Enter Copy Id : ");
                int copyId = Convert.ToInt32(Console.ReadLine());

                Console.Write("Enter New Status (Available, Borrowed, Damaged, Unavailable) : ");

                BookCopy.BookCopyStatus status =
                    Enum.Parse<BookCopy.BookCopyStatus>(Console.ReadLine()!, true);

                BookCopy updatedCopy =
                    _bookService.UpdateBookCopyStatus(copyId, status);

                Console.WriteLine("Book Copy Status Updated Successfully");
                Console.WriteLine($"Copy Id : {updatedCopy.CopyId}");
                Console.WriteLine($"New Status : {updatedCopy.Status}");
            }
            catch (InvalidBookException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DisplayBook(Book book)
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine($"Book Id        : {book.BookId}");
            Console.WriteLine($"Title          : {book.Title}");
            Console.WriteLine($"Author         : {book.Author}");
            Console.WriteLine($"ISBN           : {book.ISBN}");
            Console.WriteLine($"Category Id    : {book.CategoryId}");
            Console.WriteLine("----------------------------------");
        }
    }
}