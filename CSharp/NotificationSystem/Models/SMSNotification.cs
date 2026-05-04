using NotificationSystem.Interfaces;
using NotificationSystem.Models;

namespace NotificationSystem.Models
{
    internal class SMSNotification : INotificationMode
    {
        public bool CanSend(User user)
        {
            return !string.IsNullOrWhiteSpace(user.Phone);  
        }

        public void Send(User sender, User receiver, string message)
        {
            Console.WriteLine($"SMS sent successfully");
        }
    }
}