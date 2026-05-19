using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Models;
using LibrarySystem.UI.Session;

namespace LibrarySystem.UI.Modules.MemberModule
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
            int memberId = SessionManager.SessionMember!.MemberId;

            Console.WriteLine("====== DAMAGE LOGS ======");
            Console.WriteLine("1. View Damage Log History");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    ViewDamageHistory(memberId);
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }
        }

        private void ViewDamageHistory(int memberId)
        {
            try
            {
                List<DamageLog> damageLogs = _damageLogService.GetByMember(memberId);

                if (damageLogs.Count == 0)
                {
                    Console.WriteLine("No Damage Logs Found");
                    return;
                }

                Console.WriteLine("====== DAMAGE LOG HISTORY ======");

                foreach (var damage in damageLogs)
                {
                    Console.WriteLine("----------------------------------");
                    Console.WriteLine($"Member Id : {damage.MemberId}");
                    Console.WriteLine($"Copy Id       : {damage.CopyId}");
                    Console.WriteLine($"Description   : {damage.Description}");
                    Console.WriteLine($"Status        : {damage.status}");
                    Console.WriteLine($"Date Of Entry : {damage.DateOfEntry}");
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