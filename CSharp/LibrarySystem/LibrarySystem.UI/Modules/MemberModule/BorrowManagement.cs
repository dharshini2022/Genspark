using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using LibrarySystem.UI.Session;

namespace LibrarySystem.UI.Modules.MemberModule
{
    public class BorrowManagement
    {
        private readonly IBorrowingService _borrowingService;

        public BorrowManagement(IBorrowingService borrowingService)
        {
            _borrowingService = borrowingService;
        }

        public void Show()
        {
            int memberId = SessionManager.SessionMember!.MemberId;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("====== BORROW / RETURN ======");
            Console.ResetColor();
            Console.WriteLine("1. Borrow New Book");
            Console.WriteLine("2. Return Book");
            Console.WriteLine("3. View Borrow History");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    BorrowBook(memberId);
                    break;

                case 2:
                    ReturnBook(memberId);
                    break;

                case 3:
                    ViewBorrowHistory(memberId);
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }
        }

        private void BorrowBook(int memberId)
        {
            try
            {
                Console.Write("Enter Book Title : ");
                string title = Console.ReadLine()!;

                Borrowing borrowing =
                    _borrowingService.BorrowBookByTitle(memberId, title);

                Console.WriteLine("Book Borrowed Successfully");
                Console.WriteLine($"Borrowing Id : {borrowing.BorrowingId}");
                Console.WriteLine($"Copy Id      : {borrowing.CopyId}");
                Console.WriteLine($"Due Date     : {borrowing.DueDate}");
            }
            catch (BorrowingLimitExceededException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (FineLimitExceededException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (DuplicateActiveBorrowingException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (InvalidBookException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (InvalidMemberException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ReturnBook(int memberId)
        {
            try
            {
                Console.Write("Enter Book Title : ");
                string title = Console.ReadLine()!;

                var borrowing =_borrowingService.ReturnBookByTitle(memberId, title);

                Console.WriteLine("Book Returned Successfully");
                Console.WriteLine($"Borrowing Id : {borrowing.BorrowingId}");
                Console.WriteLine($"Returned Date: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ViewBorrowHistory(int memberId)
        {
            try
            {
                List<Borrowing> borrowings = _borrowingService.ViewBorrowHistoryByMember(memberId);

                if (borrowings.Count == 0)
                {
                    Console.WriteLine("No Borrow History Found");
                    return;
                }

                Console.WriteLine("====== BORROW HISTORY ======");

                foreach (var borrowing in borrowings)
                {
                    Console.WriteLine("----------------------------------");
                    Console.WriteLine($"Borrowing Id : {borrowing.BorrowingId}");
                    Console.WriteLine($"Copy Id      : {borrowing.CopyId}");
                    Console.WriteLine($"Borrowed At  : {borrowing.BorrowedAt}");
                    Console.WriteLine($"Due Date     : {borrowing.DueDate}");
                    Console.WriteLine($"Returned At  : {borrowing.ReturnedAt}");
                    Console.WriteLine($"Status       : {borrowing.Status}");
                    Console.WriteLine("----------------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}