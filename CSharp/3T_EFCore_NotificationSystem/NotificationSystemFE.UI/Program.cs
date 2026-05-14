using NotificationSystem.Models;
using NotificationSystem.BLL;
using NotificationSystem.DAL.Interfaces;
using NotificationSystem.DAL;
using NotificationSenders;
using NotificationSystem.Exceptions;
using System.Reflection.Metadata;



namespace NotificationSystemFE.UI
{
    internal class Program
    {
        private UserService userService;
        private NotificationService notificationService;
        private IRepository<string,User> _repo;


        private EmailNotificationSender emailMode;
        private SMSNotificationSender smsMode;
        private WhatsappNotificationSender whatsappMode;

        public Program()
        {
            _repo = new UserRepository();
            userService = new UserService(_repo);
            notificationService = new NotificationService();
            emailMode = new EmailNotificationSender();
            smsMode = new SMSNotificationSender();
            whatsappMode = new WhatsappNotificationSender();
        }

        User GetUserByEmailInput(string role)
        {
            Console.Write($"Enter {role} email id:");
            string email = Console.ReadLine() ?? "";
            return userService.GetUserByEmail(email);

        }

        User GetUserByPhoneInput(string role)
        {
            Console.Write($"Enter {role} phone number: ");
            string phone = Console.ReadLine() ?? "";
            return userService.GetUserByPhone(phone);  
        }

        User GetUserByNameInput(string role)
        {
            Console.Write($"Enter {role} Name: ");
            string name = Console.ReadLine() ?? "";
            return userService.GetUser(name) ?? throw new ContactNotFoundException($"No Contact Found with username: {name}");
        }

        string GetMessageInput()
        {
            Console.Write("Enter message: ");
            return Console.ReadLine() ?? "";
        }
        
        void SetConsoleColor(string message, string color)
        {
            if(color == "Green")
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine(message);
            Console.ResetColor();
        }
        void InitiateWorking()
        {
            bool flag = true;
            while (flag)
            {
                Console.WriteLine("\nNotification App Menu:");
                Console.WriteLine("1. Create User");
                Console.WriteLine("2. Send Email Notification");
                Console.WriteLine("3. Send SMS Notification");
                Console.WriteLine("4. Send WhatsApp Notification");
                Console.WriteLine("5. Get User by Username");
                Console.WriteLine("6. Get all Users");
                Console.WriteLine("7. Update User by Username");
                Console.WriteLine("8. Delete User by Username");
                Console.WriteLine("9. View Notifications by Username");
                Console.WriteLine("10. Exit");
                Console.Write("Choose an option (1-10): ");
                int choice;

                while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 10)
                {
                    Console.Write("Invalid Entry! Enter 1-10: ");
                }

                try
                {
                    switch (choice)
                    {
                        case 1:
                            userService.RegisterUser();
                            SetConsoleColor("User registered successfully.","Green");
                            break;

                        case 2:
                            User emailSender = GetUserByEmailInput("sender");
                            User emailReceiver = GetUserByEmailInput("receiver");
                            string emailMsg = GetMessageInput();
                            notificationService.Send( emailMode, emailSender, emailReceiver, emailMsg);                         
                            SetConsoleColor("Email notification sent successfully.","Green");
                            break;

                        case 3:
                            User smsSender = GetUserByPhoneInput("sender");
                            User smsReceiver = GetUserByPhoneInput("receiver");
                            string smsMsg = GetMessageInput();
                            notificationService.Send( smsMode, smsSender, smsReceiver, smsMsg);
                            SetConsoleColor("SMS notification sent successfully.","Green");
                            break;

                        case 4:
                            User waSender = GetUserByPhoneInput("sender");
                            User waReceiver = GetUserByPhoneInput("receiver");
                            string waMsg = GetMessageInput();
                            notificationService.Send( whatsappMode, waSender, waReceiver, waMsg);
                            SetConsoleColor("WhatsApp notification sent successfully.","Green");
                            break;

                        case 5:
                            User user = GetUserByNameInput("user");
                            Console.WriteLine(user);
                            break;

                        case 6:
                            userService.GetAllUsers();
                            break;

                        case 7:
                            User updateUser = GetUserByNameInput("user");
                            userService.UpdateUser(updateUser.Name);
                            break;

                        case 8:
                            User deleteUser = GetUserByNameInput("user");
                            userService.DeleteUser(deleteUser.Name);
                            break;

                        case 9:
                            User notifUser = GetUserByNameInput("user");
                            notificationService.PrintByUsername(notifUser);
                            break;

                        case 10:
                            Console.WriteLine("\nExiting Notification System.");
                            flag = false;
                            break;
                    }
                }

                catch (ContactNotFoundException ex)
                {
                    SetConsoleColor( $"Contact Error: {ex.Message}","Red");
                }
                catch (ExistingContactException ex)
                {
                    SetConsoleColor($"Existing Contact Error: {ex.Message}", "Red");
                }
                catch (InputFormatException ex)
                {
                    SetConsoleColor( $"Input Format Error: {ex.Message}","Red");
                }
                catch (InvalidMessageException ex)
                {
                    SetConsoleColor($"Message Error: {ex.Message}","Red");
                }
                catch (Exception ex)
                {
                    SetConsoleColor( $"Unexpected Error: {ex.Message}","Red");
                }
            }
        }

        static void Main(string[] args)
        {
            new Program().InitiateWorking();
        }

    }
}