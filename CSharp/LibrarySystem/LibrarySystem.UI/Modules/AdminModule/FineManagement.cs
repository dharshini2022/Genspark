using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Models;

namespace LibrarySystem.UI.Modules.AdminModule
{
    public class FineManagement
    {
        private readonly IFineService _fineService;

        public FineManagement(IFineService fineService)
        {
            _fineService = fineService;
        }

        public void Show()
        {

            int choice;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("======= FINE MANAGEMENT =======");
            Console.ResetColor();
            Console.WriteLine("1. Overall Active Fines");
            Console.WriteLine("2. Fine History Of Member");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int.TryParse(Console.ReadLine(), out choice);

            switch (choice)
            {
                case 1:
                    OverallActiveFines();
                    break;

                case 2:
                    FineHistoryByMember();
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }
        }

        private void OverallActiveFines()
        {
            try
            {
                List<Fine> fines = _fineService.GetAllActiveFines();

                if (fines.Count == 0)
                {
                    Console.WriteLine("No Active Fines Found");
                    return;
                }

                foreach (var fine in fines)
                {
                    DisplayFine(fine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FineHistoryByMember()
        {
            try
            {
                Console.Write("Enter Member Id : ");

                int memberId = Convert.ToInt32(Console.ReadLine());

                List<Fine> fines =
                    _fineService.GetFineHistoryByMemberId(memberId);

                if (fines.Count == 0)
                {
                    Console.WriteLine("No Fine History Found");
                    return;
                }

                foreach (var fine in fines)
                {
                    DisplayFine(fine);
                }

                decimal unpaid =
                    _fineService.GetTotalUnpaidFineByMember(memberId);

                Console.WriteLine($"\nTotal Unpaid Fine : ₹{unpaid}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DisplayFine(Fine fine)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Fine Id       : {fine.FineId}");
            Console.WriteLine($"Member Id     : {fine.MemberId}");
            Console.WriteLine($"Borrowing Id  : {fine.BorrowingId}");
            Console.WriteLine($"Amount        : ₹{fine.Amount}");
            Console.WriteLine($"Is Paid       : {fine.IsPaid}");
            Console.WriteLine("--------------------------------");
        }
    }
}