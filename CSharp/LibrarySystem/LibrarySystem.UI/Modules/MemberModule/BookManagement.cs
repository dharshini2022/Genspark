using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;

namespace LibrarySystem.UI.Modules.MemberModule
{
    public class BookManagement
    {
        private readonly IBookService _bookService;

        public BookManagement(IBookService bookService)
        {
            _bookService = bookService;
        }

        public void Show()
        {
            Console.WriteLine("====== BOOK MANAGEMENT ======");
            Console.WriteLine("1. View All Available Books");
            Console.WriteLine("2. Search Book By Title");
            Console.WriteLine("3. Search Book By Author");
            Console.WriteLine("4. Filter By Category");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    ViewAllAvailableBooks();
                    break;

                case 2:
                    SearchByTitle();
                    break;

                case 3:
                    SearchByAuthor();
                    break;

                case 4:
                    FilterByCategory();
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }
        }

        private void ViewAllAvailableBooks()
        {
            try
            {
                List<Book> books = _bookService.ViewAvailableBooks();

                if (books.Count == 0)
                {
                    Console.WriteLine("No Books Available");
                    return;
                }

                Console.WriteLine("====== AVAILABLE BOOKS ======");

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
                Console.Write("Enter Book Title : ");
                string title = Console.ReadLine()!;

                List<Book> books = _bookService.SearchBookByTitle(title);

                if (books.Count == 0)
                {
                    Console.WriteLine("No Books Found");
                    return;
                }

                Console.WriteLine("====== SEARCH RESULTS ======");

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
                Console.Write("Enter Author Name : ");
                string author = Console.ReadLine()!;

                List<Book> books = _bookService.SearchBookByAuthor(author);

                if (books.Count == 0)
                {
                    Console.WriteLine("No Books Found");
                    return;
                }

                Console.WriteLine("====== SEARCH RESULTS ======");

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

        private void FilterByCategory()
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

                Console.WriteLine("====== FILTERED BOOKS ======");

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

        private void DisplayBook(Book book)
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine($"Book Id       : {book.BookId}");
            Console.WriteLine($"Title         : {book.Title}");
            Console.WriteLine($"Author        : {book.Author}");
            Console.WriteLine($"ISBN          : {book.ISBN}");
            Console.WriteLine($"Category Id   : {book.CategoryId}");
            Console.WriteLine("----------------------------------");
        }
    }
}