using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Models;

namespace LibrarySystem.UI.Modules.AdminModule
{
    public class DamageLogManagement
    {
        private readonly IDamageLogService _damageLogService;

        public DamageLogManagement(IDamageLogService damageLogService)
        {
            _damageLogService = damageLogService;
        }

        public void Show()
        {
            Console.Clear();

            int choice;

            Console.WriteLine("======= DAMAGE LOG MANAGEMENT =======");
            Console.WriteLine("1. Create Damage Log");
            Console.WriteLine("2. View All Damage Logs");
            Console.WriteLine("3. Filter Damage Log By Member");
            Console.WriteLine("4. Filter Damage Log By Book Copy");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int.TryParse(Console.ReadLine(), out choice);

            switch (choice)
            {
                case 1:
                    CreateDamageLog();
                    break;

                case 2:
                    ViewDamageLog();
                    break;

                case 3:
                    FilterByMember();
                    break;

                case 4:
                    FilterByBook();
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private void CreateDamageLog()
        {
            try
            {
                Console.Write("Enter Borrowing Id : ");
                int borrowingId = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("Select Damage Type");
                Console.WriteLine("1. Damaged");
                Console.WriteLine("2. Lost");

                int choice = Convert.ToInt32(Console.ReadLine());

                Borrowing.BorrowingStatus status;

                switch (choice)
                {
                    case 1:
                        status = Borrowing.BorrowingStatus.Damaged;
                        break;

                    case 2:
                        status = Borrowing.BorrowingStatus.Lost;
                        break;

                    default:
                        Console.WriteLine("Invalid Choice");
                        return;
                }

                Console.Write("Enter Remarks : ");
                string remarks = Console.ReadLine()!;

                DamageLog? damage =
                    _damageLogService.ReportDamage(
                        borrowingId,
                        status,
                        remarks);

                Console.WriteLine("Damage Log Created Successfully");

                if (damage != null)
                {
                    DisplayDamageLog(damage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ViewDamageLog()
        {
            try
            {
                List<DamageLog> logs =
                    _damageLogService.GetAllDamageLogs();

                if (logs.Count == 0)
                {
                    Console.WriteLine("No Damage Logs Found");
                    return;
                }

                foreach (var log in logs)
                {
                    DisplayDamageLog(log);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FilterByMember()
        {
            try
            {
                Console.Write("Enter Member Id : ");

                int memberId = Convert.ToInt32(Console.ReadLine());

                List<DamageLog> logs =
                    _damageLogService.GetByMember(memberId);

                if (logs.Count == 0)
                {
                    Console.WriteLine("No Damage Logs Found");
                    return;
                }

                foreach (var log in logs)
                {
                    DisplayDamageLog(log);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FilterByBook()
        {
            try
            {
                Console.Write("Enter Copy Id : ");

                int copyId = Convert.ToInt32(Console.ReadLine());

                List<DamageLog> logs =
                    _damageLogService.GetByCopy(copyId);

                if (logs.Count == 0)
                {
                    Console.WriteLine("No Damage Logs Found");
                    return;
                }

                foreach (var log in logs)
                {
                    DisplayDamageLog(log);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DisplayDamageLog(DamageLog log)
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine($"Damage Id     : {log.DamageId}");
            Console.WriteLine($"Member Id     : {log.MemberId}");
            Console.WriteLine($"Copy Id       : {log.CopyId}");
            Console.WriteLine($"Description   : {log.Description}");
            Console.WriteLine($"Status        : {log.status}");
            Console.WriteLine($"Date Of Entry : {log.DateOfEntry}");
            Console.WriteLine("----------------------------------");
        }
    }
}