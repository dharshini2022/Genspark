using LibrarySystem.UI.Modules.MemberModule;
using LibrarySystem.UI.Session;

namespace LibrarySystem.UI.Menus
{
    public class MemberMenu
    {
        private readonly DashboardManagement _dashboardManagement;
        private readonly ProfileManagement _profileManagement;
        private readonly BookManagement _bookManagement;
        private readonly BorrowManagement _borrowManagement;
        private readonly FineManagement _fineManagement;
        private readonly DamageLogManagement _damageLogManagement;

        public MemberMenu(
            DashboardManagement dashboardManagement,
            ProfileManagement profileManagement,
            BookManagement bookManagement,
            BorrowManagement borrowManagement,
            FineManagement fineManagement,
            DamageLogManagement damageLogManagement)
        {
            _dashboardManagement = dashboardManagement;
            _profileManagement = profileManagement;
            _bookManagement = bookManagement;
            _borrowManagement = borrowManagement;
            _fineManagement = fineManagement;
            _damageLogManagement = damageLogManagement;
        }

        public void Show()
        {
            bool logout = false;

            while (!logout)
            {
                Console.WriteLine();
                Console.WriteLine($"Welcome {SessionManager.SessionMember?.Name}");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("====== MEMBER MENU ======");
                Console.ResetColor();
                Console.WriteLine("1. Dashboard Reports");
                Console.WriteLine("2. Profile Management");
                Console.WriteLine("3. Books Management");
                Console.WriteLine("4. Borrow / Return");
                Console.WriteLine("5. Fine Management");
                Console.WriteLine("6. Damage Logs");
                Console.WriteLine("7. Logout");
                int choice;
                Console.Write("Enter Choice : ");
                while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 7)
                {
                    Console.Write("Invalid Entry! Enter 1-10: ");
                }

                switch (choice)
                {
                    case 1:
                        _dashboardManagement.Show();
                        break;

                    case 2:
                        _profileManagement.Show();
                        break;

                    case 3:
                        _bookManagement.Show();
                        break;

                    case 4:
                        _borrowManagement.Show();
                        break;

                    case 5:
                        _fineManagement.Show();
                        break;

                    case 6:
                        _damageLogManagement.Show();
                        break;

                    case 7:
                        SessionManager.LogOut();
                        logout = true;
                        break;

                    default:
                        Console.WriteLine("Invalid Choice");
                        break;
                }
            }
        }
    }
}