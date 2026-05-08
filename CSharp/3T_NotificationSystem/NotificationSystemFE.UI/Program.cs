using NotificationSystem.Models;
using NotificationSystem.BLL;
using NotificationSystem.DAL.Interfaces;
using NotificationSystem.DAL;
using NotificationSenders;


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
                while (!int.TryParse(Console.ReadLine(), out choice) && choice < 1 && choice > 10)
                {
                    Console.Write("  Invalid Entry! Enter a number between 1 and 10: ");
                }

                switch (choice)
                {
                    case 1:
                        userService.RegisterUser();
                        break;

                    case 2:
                        Console.Write("  Enter sender's mail id: ");
                        string emailSenderName = Console.ReadLine() ?? "";
                        // User? emailSender = userService.GetUserByEmail(emailSenderName);
                        // if (emailSender == null) { 
                        //     Console.WriteLine($"User not found with mail : {emailSender}");
                        //     break; 
                        // }

                        Console.Write("  Enter receiver's mail id: ");
                        string emailReceiverName = Console.ReadLine() ?? "";
                        User? emailReceiver = userService.GetUserByEmail(emailReceiverName);
                        // if (emailReceiver == null) { 
                        //     Console.WriteLine($"User not found with email : {emailReceiver}");
                        //     break; 
                        // }

                        Console.Write("  Enter message: ");
                        string emailMsg = Console.ReadLine() ?? "";
                        notificationService.Send(emailMode, emailSender, emailReceiver, emailMsg);
                        break;

                    case 3:
                        Console.Write("  Enter sender's phone: ");
                        string smsSenderName = Console.ReadLine() ?? "";
                        User? smsSender = userService.GetUserByPhone(smsSenderName);
                        // if (smsSender == null) { 
                        //     Console.WriteLine($"User not found with phone : {smsSender}");
                        //     break; }

                        Console.Write("  Enter receiver's phone: ");
                        string smsReceiverName = Console.ReadLine() ?? "";
                        User? smsReceiver = userService.GetUserByPhone(smsReceiverName);
                        // if (smsReceiver == null) { 
                        //     Console.WriteLine($"User not found with phone: {smsReceiver}");
                        //     break; 
                        // }

                        Console.Write("  Enter message: ");
                        string smsMsg = Console.ReadLine() ?? "";
                        notificationService.Send(smsMode, smsSender, smsReceiver, smsMsg);
                        break;

                    case 4:
                        Console.Write("  Enter sender's WhatsApp Number: ");
                        string waSenderName = Console.ReadLine() ?? "";
                        User? waSender = userService.GetUserByPhone(waSenderName);
                        // if (waSender == null) { break; }

                        Console.Write("  Enter receiver's WhatsApp Number: ");
                        string waReceiverName = Console.ReadLine() ?? "";
                        User? waReceiver = userService.GetUserByPhone(waReceiverName);
                        // if (waReceiver == null) { break; }

                        Console.Write("  Enter message: ");
                        string waMsg = Console.ReadLine() ?? "";
                        notificationService.Send(whatsappMode, waSender, waReceiver, waMsg);
                        break;
                                        
                    case 5:
                        Console.Write("Enter username: ");
                        string username = Console.ReadLine() ?? "";
                        userService.GetUserByName(username);
                        break;

                    case 6:
                        userService.GetAllUsers();
                        break;

                    case 7:
                        Console.WriteLine("Enter Username: ");
                        string updateUserName = Console.ReadLine() ?? "";
                        userService.UpdateUser(updateUserName);
                        break;

                    case 8:
                        Console.Write("Enter Username: ");
                        string deleteUsername = Console.ReadLine() ?? "";
                        userService.DeleteUser(deleteUsername);
                        break;

                    case 9:
                        Console.Write("  Enter username: ");
                        string notifUsername = Console.ReadLine() ?? "";
                        User? notifUser = userService.GetUser(notifUsername); 
                        if (notifUser == null) 
                        { 
                            Console.WriteLine($"  User '{notifUsername}' not found."); 
                            break; 
                        }
                        notificationService.PrintByUsername(notifUser);      
                        break;
                                        
                    case 10:
                        Console.WriteLine("\nExiting Notification System.");
                        flag = false;
                        break;

                    default:
                        Console.WriteLine("Invalid Entry! Enter a number between 1 to 10");
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            new Program().InitiateWorking();
        }
    }
}