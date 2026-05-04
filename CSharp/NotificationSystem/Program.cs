using NotificationSystem.Models;
using NotificationSystem.Services;

namespace NotificationSystem
{
    internal class Program
    {
        private UserService userService;
        private NotificationService notificationService;


        private EmailNotification emailMode;
        private SMSNotification smsMode;
        private WhatsappNotification whatsappMode;

        public Program()
        {
            userService = new UserService();
            notificationService = new NotificationService();
            emailMode = new EmailNotification();
            smsMode = new SMSNotification();
            whatsappMode = new WhatsappNotification();
        }

        void InitiateWorking()
        {
            bool flag = true;
            while (flag)
            {
                Console.WriteLine("\nNotification App Menu:");
                Console.WriteLine("1. Register User");
                Console.WriteLine("2. Send Email Notification");
                Console.WriteLine("3. Send SMS Notification");
                Console.WriteLine("4. Send WhatsApp Notification");
                Console.WriteLine("5. Find User by Email");
                Console.WriteLine("6. Find User by Phone");
                Console.WriteLine("7. Delete a User by Email");
                Console.WriteLine("8. Delete a User by Phone");
                Console.WriteLine("9. Exit");
                Console.Write("Choose an option (1-9): ");

                int choice;
                while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 9)
                {
                    Console.Write("  Invalid Entry! Enter a number between 1 and 9: ");
                }

                switch (choice)
                {
                    case 1:
                        User newUser = userService.RegisterUser();
                        Console.WriteLine("\n  User registered successfully!");
                        userService.PrintUserDetails(newUser);
                        break;

                    case 2:
                        Console.Write("Enter sender's email: ");
                        string senderEmail = Console.ReadLine()?.Trim() ?? "";
                        User? emailSender = userService.GetUserByEmail(senderEmail);
                        if (emailSender == null) break;

                        Console.Write("Enter receiver's email: ");
                        string receiverEmail = Console.ReadLine()?.Trim() ?? "";
                        User? emailReceiver = userService.GetUserByEmail(receiverEmail);
                        if (emailReceiver == null) break;

                        Console.Write("Enter your message: ");
                        string emailMsg = Console.ReadLine()?.Trim() ?? "";
                        notificationService.Send(emailMode, emailSender, emailReceiver, emailMsg, Notification.NotificationType.Email);
                        break;

                    case 3:
                        Console.Write("Enter sender's phone: ");
                        string senderPhone = Console.ReadLine()?.Trim() ?? "";
                        User? smsSender = userService.GetUserByPhone(senderPhone);
                        if (smsSender == null) break;

                        Console.Write("Enter receiver's phone: ");
                        string receiverPhone = Console.ReadLine()?.Trim() ?? "";
                        User? smsReceiver = userService.GetUserByPhone(receiverPhone);
                        if (smsReceiver == null) break;

                        Console.Write("Enter your message: ");
                        string smsMsg = Console.ReadLine()?.Trim() ?? "";
                        notificationService.Send(smsMode, smsSender, smsReceiver, smsMsg, Notification.NotificationType.SMS);
                        break;

                    case 4:
                        Console.Write("Enter sender's phone: ");
                        string waSenderPhone = Console.ReadLine()?.Trim() ?? "";
                        User? waSender = userService.GetUserByPhone(waSenderPhone);
                        if (waSender == null) break;

                        Console.Write("Enter receiver's phone: ");
                        string waReceiverPhone = Console.ReadLine()?.Trim() ?? "";
                        User? waReceiver = userService.GetUserByPhone(waReceiverPhone);
                        if (waReceiver == null) break;

                        Console.Write("Enter your message: ");
                        string waMsg = Console.ReadLine()?.Trim() ?? "";
                        notificationService.Send(whatsappMode, waSender, waReceiver, waMsg, Notification.NotificationType.WhatsApp);
                        break;

                    case 5:
                        Console.Write("Enter email: ");
                        string findEmail = Console.ReadLine()?.Trim() ?? "";
                        User? findByEmailUser = userService.GetUserByEmail(findEmail);
                        if (findByEmailUser != null) userService.PrintUserDetails(findByEmailUser);
                        break;

                    case 6:
                        Console.Write("Enter phone: ");
                        string findPhone = Console.ReadLine()?.Trim() ?? "";
                        User? findByPhoneUser = userService.GetUserByPhone(findPhone);
                        if (findByPhoneUser != null) userService.PrintUserDetails(findByPhoneUser);
                        break;

                    case 7:
                        Console.Write("Enter email: ");
                        string deleteEmail = Console.ReadLine()?.Trim() ?? "";
                        User? deleteByEmailUser = userService.GetUserByEmail(deleteEmail);
                        if (deleteByEmailUser == null){
                            Console.WriteLine($"User not found with email : {deleteEmail}");
                            break;
                        }
                        userService.DeleteUser(deleteByEmailUser);
                        break;

                    case 8:
                        Console.Write("Enter phone: ");
                        string deletePhone = Console.ReadLine()?.Trim() ?? "";
                        User? deleteByPhoneUser = userService.GetUserByPhone(deletePhone);
                        if (deleteByPhoneUser == null){
                            Console.WriteLine($"User not found with phone : {deletePhone}");
                            break;
                        }
                        userService.DeleteUser(deleteByPhoneUser);
                        break;
                    
                    case 9:
                        Console.WriteLine("\nExiting Notification System.");
                        flag = false;
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