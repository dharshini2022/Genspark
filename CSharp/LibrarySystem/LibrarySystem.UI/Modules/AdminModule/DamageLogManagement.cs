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

            int choice;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("======= DAMAGE LOG MANAGEMENT =======");
            Console.ResetColor();
            Console.WriteLine("1. View All Damage Logs");
            Console.WriteLine("2. Filter Damage Log By Member");
            Console.WriteLine("3. Filter Damage Log By Book Copy");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int.TryParse(Console.ReadLine(), out choice);

            switch (choice)
            {
                case 1:
                    ViewDamageLog();
                    break;

                case 2:
                    FilterByMember();
                    break;

                case 3:
                    FilterByBook();
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
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