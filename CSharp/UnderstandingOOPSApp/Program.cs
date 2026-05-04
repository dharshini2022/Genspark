using UnderstandingOOPSApp.Interfaces;
using UnderstandingOOPSApp.Services;
using UnderstandingOOPSApp.Models;

namespace BankApp
{
    internal class Program
    {
        ICustomerInteract customerInteract;
        public Program()
        {
            customerInteract = new CustomerService();
        }
        void DoBanking()
        {
            bool status = true;
            while (status)
            {
                Console.WriteLine("\nBank Menu:");
                Console.WriteLine("1. Add Account");
                Console.WriteLine("2. Print Account by Account Number");
                Console.WriteLine("3. Print Account by Phone Number");
                Console.WriteLine("4. Exit");
                Console.Write("Enter your choice: ");

                int choice;
                while(!int.TryParse(Console.ReadLine(), out choice) && choice>0 && choice < 4)
                {
                    Console.WriteLine("Invalid Input");
                }

                switch (choice)
                {
                    case 1:
                        var acc = customerInteract.OpensAccount();
                        Console.WriteLine("Account Created Successfully");
                        Console.WriteLine(acc);
                        break;
                    case 2:
                        Console.WriteLine("Please Enter Account Number:");
                        string AccNo = Console.ReadLine() ?? "";
                        customerInteract.PrintAccountDetailsByAccNo(AccNo);
                        break;
                    case 3:
                        Console.Write("Enter Phone Number: ");
                        string phone = Console.ReadLine() ?? "";
                        customerInteract.PrintAccountDetailsByPhone(phone);
                        break;

                    case 4:
                        status = false;
                        Console.WriteLine("Exisiting");
                        break;
                    default:
                       Console.WriteLine("Invalid Entry");
                       break;
                }

            }
            
        }
        static void Main(string[] args)
        {
            new Program().DoBanking();
        }
    }
}
