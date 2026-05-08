using NotificationSystem.BLL.Interfaces;
using NotificationSystem.Models;

namespace NotificationSenders
{
    public class EmailNotificationSender : INotificationSender
    {
        public bool CanSend(User sender, User receiver)
        {
            return !string.IsNullOrWhiteSpace(sender.Email) && !string.IsNullOrWhiteSpace(receiver.Email);
        }

        public Notification? Send(User sender, User receiver, string message)
        {
            if (!CanSend(sender, receiver))
            {
                Console.WriteLine("Failed — missing email on sender or receiver.");
                return null;
            }

            Console.WriteLine("Email sent successfully!");
            Console.WriteLine($"  Sender:  {sender.Email}\n Receiver: {receiver.Email}\n Message: {message}");

            return new Notification(message, sender, receiver, Notification.NotificationType.Email, sender.Email, receiver.Email);
        }
    }
}