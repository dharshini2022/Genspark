using LibrarySystem.BLL.Interfaces;
using LibrarySystem.UI.Session;

namespace LibrarySystem.UI.Modules.MemberModule
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
            int memberId = SessionManager.SessionMember!.MemberId;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("====== DASHBOARD REPORTS ======");
            Console.ResetColor();
            Console.WriteLine("1. Most Borrowed Books");
            Console.WriteLine("2. Overdue Books");
            Console.WriteLine("3. Pending Fines");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch(choice)
            {
                case 1:
                    MostBorrowedBook(memberId);
                    break;

                case 2:
                    OverdueBooks(memberId);
                    break;

                case 3:
                    PendingFines(memberId);
                    break;
            }
        }

        private void MostBorrowedBook(int memberId)
        {
            var books = _dashboardService.GetMostBorrowedBooks();

            Console.WriteLine("\n===== MOST BORROWED BOOKS =====");

            foreach(var book in books)
            {
                Console.WriteLine($"Book Id      : {book.BookId}");
                Console.WriteLine($"Title        : {book.Title}");
                Console.WriteLine($"Author       : {book.Author}");
                Console.WriteLine($"ISBN         : {book.ISBN}");
                Console.WriteLine($"Category     : {book.CategoryName}");
                Console.WriteLine($"Borrow Count : {book.BorrowCount}");
                Console.WriteLine("--------------------------------");
            }
        }

        private void OverdueBooks(int memberId)
        {
            var overdues = _dashboardService.GetOverdueBooksByMember(memberId);

            Console.WriteLine("\n===== OVERDUE BOOKS =====");

            if(overdues.Count == 0)
            {
                Console.WriteLine("No overdue books found.");
                return;
            }

            foreach(var item in overdues)
            {
                Console.WriteLine($"Borrowing Id : {item.BorrowingId}");
                Console.WriteLine($"Book Title   : {item.BookTitle}");
                Console.WriteLine($"Copy Code    : {item.CopyCode}");
                Console.WriteLine($"Borrowed At  : {item.BorrowedAt}");
                Console.WriteLine($"Due Date     : {item.DueDate}");
                Console.WriteLine($"Days Overdue : {item.DaysOverdue}");
                Console.WriteLine("--------------------------------");
            }
        }

        private void PendingFines(int memberId)
        {
            decimal amount = _dashboardService.GetPendingFine(memberId);

            Console.WriteLine("\n===== PENDING FINES =====");
            Console.WriteLine($"Total Pending Fine : Rs.{amount}");
        }
    }
}