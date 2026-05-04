using NotificationSystem.Interfaces;
using NotificationSystem.Models;

namespace NotificationSystem.Models
{

    internal class WhatsappNotification : INotificationMode
    {
        public bool CanSend(User user)
        {
            return !string.IsNullOrWhiteSpace(user.Phone) && user.HasWhatsapp;
        }

        public void Send(User sender, User receiver, string message)
        {
            Console.WriteLine($"Whatsapp notification send successfully");
        }
    }
}