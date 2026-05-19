using LibrarySystem.BLL.Interfaces;
using LibrarySystem.UI.Session;

namespace LibrarySystem.UI.Modules.MemberModule
{
    public class DashboardManagement
    {
        private readonly IFineService _fineService;
        public DashboardManagement(IFineService fineService)
        {
            _fineService = fineService;
        }
        public void Show()
        {
            int memberId = SessionManager.SessionMember!.MemberId;

            Console.WriteLine("====== DASHBOARD REPORTS ======");
            Console.WriteLine("1. Most Borrowed Book");
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

        }

        private void OverdueBooks(int memberId)
        {

        }

        private void PendingFines(int memberId)
        {
            decimal amount = _fineService.GetTotalUnpaidFineByMember(memberId);

            Console.WriteLine("\n===== PENDING FINES =====");
            Console.WriteLine($"Total Pending Fine : Rs.{amount}");
        }
    }
}