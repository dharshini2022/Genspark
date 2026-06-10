using Database_Approach_EFCore.Context;
using Database_Approach_EFCore.Models;
using Microsoft.EntityFrameworkCore;

namespace Database_Approach_EFCore
{
    internal class Program
    {
        readonly DemoContext _context;
        Program()
        {
            _context = new DemoContext();
        }

        void TransactWithTransactioninDatabase()
        {
            Account fc = new Account() { Accno = 2 };
            Account tc = new Account() { Accno = 3};
            float amount = 100;
            int tran_id = 7;
            fc = _context.Accounts.FirstOrDefault(a => a.Accno == fc.Accno);
            tc = _context.Accounts.FirstOrDefault(a => a.Accno == tc.Accno);
            using var transaction = _context.Database.BeginTransaction();
            try
            {

                _context.Database.ExecuteSqlInterpolated($"call add_trans({tran_id},{fc.Accno},{tc.Accno},{amount})");
                if (fc.Balance - amount <= 0)
                    throw new Exception("Insuffienct balance");
                _context.Database.ExecuteSqlInterpolated($"call update_account({fc.Accno},{fc.Balance-amount})");
                _context.Database.ExecuteSqlInterpolated($"call update_account({tc.Accno},{tc.Balance + amount})");
                transaction.Commit();
                Console.WriteLine("Transaction successfull");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine(ex.Message);
            }
        }
        void AddAccountUsingSP()
        {
            Account account = new Account() {Accno = 5, Name = "SP2", Balance = 100 };
            //call add_account(4,3243);
            _context.Database.ExecuteSqlInterpolated($"call add_account({account.Accno},{account.Name},{account.Balance});");
            Console.WriteLine("Account Created");
        }
        static void Main(string[] args)
        {
            new Program().TransactWithTransactioninDatabase();
        }
    }
}