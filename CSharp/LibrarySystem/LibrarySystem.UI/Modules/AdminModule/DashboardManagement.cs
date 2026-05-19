using LibrarySystem.BLL.Interfaces;

namespace LibrarySystem.UI.Modules.AdminModule
{
    public class DashboardManagement
    {
        private readonly IDashboardService _dashboardService;

        public DashboardManagement(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public void Show()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("====== DASHBOARD REPORTS ======");
            Console.ResetColor();

            Console.WriteLine("1. Overall Borrowed Books");
            Console.WriteLine("2. Overall Overdue Books");
            Console.WriteLine("3. Members with Pending Fines");
            Console.WriteLine("4. Most Borrowed Book");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch(choice)
            {
                case 1:
                    OverallBorrowedBooks();
                    break;

                case 2:
                    OverallOverdueBooks();
                    break;

                case 3:
                    MembersWithPendingFines();
                    break;

                case 4:
                    MostBorrowedBook();
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice!");
                    break;
            }
        }

        private void OverallBorrowedBooks()
        {
            var borrowedBooks = _dashboardService.GetOverallBorrowedBooks();

            Console.WriteLine("\n====== OVERALL BORROWED BOOKS ======");

            if(!borrowedBooks.Any())
            {
                Console.WriteLine("No borrowed books found.");
                return;
            }

            foreach(var item in borrowedBooks)
            {
                Console.WriteLine($"Borrowing Id : {item.BorrowingId}");
                Console.WriteLine($"Member Name  : {item.MemberName}");
                Console.WriteLine($"Book Title   : {item.BookTitle}");
                Console.WriteLine($"Copy Code    : {item.CopyCode}");
                Console.WriteLine($"Borrowed At  : {item.BorrowedAt}");
                Console.WriteLine($"Due Date     : {item.DueDate}");
                Console.WriteLine($"Status       : {item.Status}");
                Console.WriteLine("-----------------------------------");
            }
        }

        private void OverallOverdueBooks()
        {
            var overdueBooks = _dashboardService.GetOverallOverdueBooks();

            Console.WriteLine("\n====== OVERALL OVERDUE BOOKS ======");

            if(!overdueBooks.Any())
            {
                Console.WriteLine("No overdue books found.");
                return;
            }

            foreach(var item in overdueBooks)
            {
                Console.WriteLine($"Borrowing Id : {item.BorrowingId}");
                Console.WriteLine($"Member Name  : {item.MemberName}");
                Console.WriteLine($"Book Title   : {item.BookTitle}");
                Console.WriteLine($"Copy Code    : {item.CopyCode}");
                Console.WriteLine($"Due Date     : {item.DueDate}");
                Console.WriteLine($"Days Overdue : {item.DaysOverdue}");
                Console.WriteLine("-----------------------------------");
            }
        }

        private void MembersWithPendingFines()
        {
            var members = _dashboardService.GetMembersWithPendingFines();

            Console.WriteLine("\n====== MEMBERS WITH PENDING FINES ======");

            if(!members.Any())
            {
                Console.WriteLine("No pending fines found.");
                return;
            }

            foreach(var item in members)
            {
                Console.WriteLine($"Member Id    : {item.MemberId}");
                Console.WriteLine($"Member Name  : {item.MemberName}");
                Console.WriteLine($"Email        : {item.Email}");
                Console.WriteLine($"Pending Fine : Rs.{item.PendingFine}");
                Console.WriteLine("-----------------------------------");
            }
        }

        private void MostBorrowedBook()
        {
            var books = _dashboardService.GetMostBorrowedBooks();

            Console.WriteLine("\n====== MOST BORROWED BOOKS ======");

            if(!books.Any())
            {
                Console.WriteLine("No records found.");
                return;
            }

            foreach(var book in books)
            {
                Console.WriteLine($"Book Id      : {book.BookId}");
                Console.WriteLine($"Title        : {book.Title}");
                Console.WriteLine($"Author       : {book.Author}");
                Console.WriteLine($"ISBN         : {book.ISBN}");
                Console.WriteLine($"Category     : {book.CategoryName}");
                Console.WriteLine($"Borrow Count : {book.BorrowCount}");
                Console.WriteLine("-----------------------------------");
            }
        }
    }
}