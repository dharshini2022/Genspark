using NotificationSystem.BLL.Interfaces;
using NotificationSystem.Models;

namespace NotificationSenders
{

    public class WhatsappNotificationSender : INotificationSender
    {

        public bool CanSend(User sender, User receiver)
        {
            return !string.IsNullOrWhiteSpace(sender.Phone) && !string.IsNullOrWhiteSpace(receiver.Phone);
        }

        public Notification? Send(User sender, User receiver, string message)
        {
            if (!CanSend(sender, receiver) || !sender.HasWhatsapp || !receiver.HasWhatsapp)
            {
                Console.WriteLine("Failed — missing Whatsapp Phone Number on sender or receiver.");
                return null;
            }

            Console.WriteLine("Whatsapp Message sent successfully!");
            Console.WriteLine($"  Sender:  {sender.Phone}\n Receiver: {receiver.Phone}\n Message: {message}");

            return new Notification(message, sender, receiver, Notification.NotificationType.WhatsApp, sender.Phone, receiver.Phone);
        }
    }
}