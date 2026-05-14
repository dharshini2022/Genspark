using NotificationSystem.BLL.Interfaces;
using NotificationSystem.Models;

namespace NotificationSenders
{
    public class SMSNotificationSender : INotificationSender
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

            return new Notification(message, sender.Id, receiver.Id, Notification.NotificationType.SMS, sender.Phone, receiver.Phone);
        }
    }
}