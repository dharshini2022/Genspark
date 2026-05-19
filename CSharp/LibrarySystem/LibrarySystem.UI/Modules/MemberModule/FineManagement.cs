using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using LibrarySystem.UI.Session;

namespace LibrarySystem.UI.Modules.MemberModule
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
            int memberId = SessionManager.SessionMember!.MemberId;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("====== FINE MANAGEMENT ======");
            Console.ResetColor();
            Console.WriteLine("1. View Fine History");
            Console.WriteLine("2. Pay Fine");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    ViewFineHistory(memberId);
                    break;

                case 2:
                    PayFine(memberId);
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }
        }

        private void ViewFineHistory(int memberId)
        {
            try
            {
                List<Fine> fines = _fineService.GetFineHistoryByMemberId(memberId);

                if (fines.Count == 0)
                {
                    Console.WriteLine("No Fine History Found");
                    return;
                }

                decimal totalUnpaidFine = _fineService.GetTotalUnpaidFineByMember(memberId);

                Console.WriteLine("====== FINE HISTORY ======");

                foreach (var fine in fines)
                {
                    Console.WriteLine("----------------------------------");
                    Console.WriteLine($"Fine Id       : {fine.FineId}");
                    Console.WriteLine($"Borrowing Id  : {fine.BorrowingId}");
                    Console.WriteLine($"Amount        : ₹{fine.Amount}");
                    Console.WriteLine($"Paid Status   : {(fine.IsPaid ? "Paid" : "Unpaid")}");
                    Console.WriteLine($"Paid At       : {fine.PaidAt}");
                    Console.WriteLine("----------------------------------");
                }

                Console.WriteLine($"Total Unpaid Fine : ₹{totalUnpaidFine}");
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

        private void PayFine(int memberId)
        {
            try
            {
                List<Fine> fines = _fineService.GetFineHistoryByMemberId(memberId);

                var unpaidFines = fines
                    .Where(f => !f.IsPaid)
                    .ToList();

                if (unpaidFines.Count == 0)
                {
                    Console.WriteLine("No Pending Fines");
                    return;
                }

                Console.WriteLine("====== UNPAID FINES ======");

                foreach (var fine in unpaidFines)
                {
                    Console.WriteLine("----------------------------------");
                    Console.WriteLine($"Fine Id      : {fine.FineId}");
                    Console.WriteLine($"Borrowing Id : {fine.BorrowingId}");
                    Console.WriteLine($"Amount       : ₹{fine.Amount}");
                    Console.WriteLine("----------------------------------");
                }

                Console.Write("Enter Fine Id To Pay : ");

                int fineId = Convert.ToInt32(Console.ReadLine());

                Fine paidFine = _fineService.PayFine(fineId);

                Console.WriteLine("Fine Paid Successfully");
                Console.WriteLine($"Fine Id : {paidFine.FineId}");
                Console.WriteLine($"Amount  : ₹{paidFine.Amount}");
                Console.WriteLine($"Paid At : {paidFine.PaidAt}");
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
    }
}