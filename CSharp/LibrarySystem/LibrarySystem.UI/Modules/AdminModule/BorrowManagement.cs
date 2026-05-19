using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Models;

namespace LibrarySystem.UI.Modules.AdminModule
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

            int choice;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("======= BORROW MANAGEMENT =======");
            Console.ResetColor();
            Console.WriteLine("1. View Active Borrows");
            Console.WriteLine("2. Approve Returns");
            Console.WriteLine("3. View Borrow History Of Member");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int.TryParse(Console.ReadLine(), out choice);

            switch (choice)
            {
                case 1:
                    ActiveBorrows();
                    break;

                case 2:
                    ApproveReturns();
                    break;

                case 3:
                    BorrowHistoryOfMember();
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }
        }

        private void ActiveBorrows()
        {
            try
            {
                List<Borrowing> borrows = _borrowingService.ViewActiveBorrows();

                if (borrows.Count == 0)
                {
                    Console.WriteLine("No Active Borrows");
                    return;
                }

                foreach (var borrow in borrows)
                {
                    DisplayBorrow(borrow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ApproveReturns()
        {
            try
            {
                Console.Write("Enter Borrowing Id : ");
                int borrowingId = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("Select Status");
                Console.WriteLine("1. Returned");
                Console.WriteLine("2. Damaged");
                Console.WriteLine("3. Lost");

                int statusChoice = Convert.ToInt32(Console.ReadLine());

                Borrowing.BorrowingStatus status;

                switch (statusChoice)
                {
                    case 1:
                        status = Borrowing.BorrowingStatus.Returned;
                        break;

                    case 2:
                        status = Borrowing.BorrowingStatus.Damaged;
                        break;

                    case 3:
                        status = Borrowing.BorrowingStatus.Lost;
                        break;

                    default:
                        Console.WriteLine("Invalid Status");
                        return;
                }

                Console.Write("Enter Remarks : ");
                string remarks = Console.ReadLine()!;

                Borrowing borrowing =_borrowingService.ApproveReturn(borrowingId,status,remarks);

                Console.WriteLine("Return Approved Successfully");

                DisplayBorrow(borrowing);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void BorrowHistoryOfMember()
        {
            try
            {
                Console.Write("Enter Member Id : ");

                int memberId = Convert.ToInt32(Console.ReadLine());

                List<Borrowing> borrows =
                    _borrowingService.ViewBorrowHistoryByMember(memberId);

                if (borrows.Count == 0)
                {
                    Console.WriteLine("No Borrow History Found");
                    return;
                }

                foreach (var borrow in borrows)
                {
                    DisplayBorrow(borrow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DisplayBorrow(Borrowing borrow)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Borrowing Id : {borrow.BorrowingId}");
            Console.WriteLine($"Member Id    : {borrow.MemberId}");
            Console.WriteLine($"Copy Id      : {borrow.CopyId}");
            Console.WriteLine($"Borrowed At  : {borrow.BorrowedAt}");
            Console.WriteLine($"Due Date     : {borrow.DueDate}");
            Console.WriteLine($"Returned At  : {borrow.ReturnedAt}");
            Console.WriteLine($"Status       : {borrow.Status}");
            Console.WriteLine("--------------------------------");
        }
    }
}