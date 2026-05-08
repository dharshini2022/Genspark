using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnderstandingOOPSApp.Interfaces;
using UnderstandingOOPSApp.Models;
using UnderstandingOOPSApp.Repositories;

namespace UnderstandingOOPSApp.Services
{
    internal class CustomerService : ICustomerInteract
    {
        //static used so AccountNumber values is retained globally for all objs.

        private IRepository<string,Account> _accountRepo;
        public CustomerService(IRepository<string,Account> AccountRepository)
        {
            _accountRepo = AccountRepository;
        }
        public Account OpensAccount()
        {
            Account account = TakeCustomerDetails();
            TakeInitialDeposit(account);
            return _accountRepo.Create(account);
        }

        private void TakeInitialDeposit(Account account)
        {
            Console.WriteLine("Do you want to deposit any amount now. If yes enter the amount. else enter 0.?");
            float amount = 0;
            while(!float.TryParse(Console.ReadLine(), out amount))
                Console.WriteLine("Invalid entry. Please enter the deposit amount");
            account.Balance += amount;

        }

        private Account TakeCustomerDetails()
        {
            Account account = new Account();
            Console.WriteLine("Please select the type of account you want to open. 1 for savings. 2 for current");
            int typeChoice;
            while(!int.TryParse(Console.ReadLine(), out typeChoice) && typeChoice>0 && typeChoice<3)
                Console.WriteLine("Invalid entry. Please try again");
            if(typeChoice == 1)
                account = new SavingAccount();
            if(typeChoice == 2)
                account = new CurrentAccount();

            Console.WriteLine("Please enter your full name");
            account.NameOnAccount = Console.ReadLine()??"";

            Console.WriteLine("Please enter your Date of birth in yyyy-mm--dd format");
            DateTime dob;
            while(!DateTime.TryParse(Console.ReadLine(),out dob ) && dob>DateTime.Today)
                Console.WriteLine("Invalid entry for date. Please try again");

            Console.WriteLine("Please enter your email address");
            string email = Console.ReadLine() ?? "";
            while (!(email.Contains('@') && email.EndsWith(".com")))
            {
                Console.WriteLine("Invalid Email Entry! Try again:");
                email = Console.ReadLine() ?? "";
            }
            account.Email = email;

            Console.WriteLine("Please enter your phone number");
            string phone = Console.ReadLine() ?? "";

            while (phone.Length != 10 || !phone.All(char.IsDigit))
            {
                Console.WriteLine("Invalid Phone Number! Enter 10 digits only:");
                phone = Console.ReadLine() ?? "";
            }
            account.Phone = phone;
            return account;
        }

        public void PrintAccountDetailsByAccNo(string accountNumber)
        {
            var account = _accountRepo.GetAccount(accountNumber);
            if (account == null)
            {
                Console.WriteLine("No account found");
                return;
            }
            PrintAccount(account);
        }

        public void GetAccounts()
        {
            List<Account>? AllAccounts = _accountRepo.GetAccounts();
            if(AllAccounts == null)
            {
                Console.WriteLine("Empty set! No Accounts present");
                return;
            }
            foreach(Account acc in AllAccounts)
            {
                PrintAccount(acc);
            }
        }

        // public void PrintAccountDetailsByPhone(string Phone)
        // {
        //     foreach (var item in accounts)
        //     {
        //         if(item.Phone == Phone)
        //         {
        //             Account account = item;
        //             PrintAccount(account);
        //             return;
        //         }
        //     }
        //     Console.WriteLine("No account with the given phone number is present with us");
            
        // }

        private void PrintAccount(Account account)
        {
            Console.WriteLine("-----------------------------");
            Console.WriteLine(account);
            Console.WriteLine("-----------------------------");
        }
    }
}
