using NotificationSystem.Interfaces;
using NotificationSystem.Models;

namespace NotificationSystem.Models
{
    internal class SMSNotification : INotification
    {
        public bool CanSend(User sender, User receiver)
        {
            return !string.IsNullOrWhiteSpace(sender.Phone) && !string.IsNullOrWhiteSpace(receiver.Phone);
        }

        public Notification? Send(User sender, User receiver, string message)
        {
            if (!CanSend(sender, receiver))
            {
                Console.WriteLine("Failed — missing SMS Phone Number on sender or receiver.");
                return null;
            }

            Console.WriteLine("SMS sent successfully!");
            Console.WriteLine($"  Sender:  {sender.Phone}\n Receiver: {receiver.Phone}\n Message: {message}");

            return new Notification(message, sender, receiver, Notification.NotificationType.SMS, sender.Phone, receiver.Phone);
        }
    }
}