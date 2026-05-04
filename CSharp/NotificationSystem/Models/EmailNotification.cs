using NotificationSystem.Interfaces;
using NotificationSystem.Models;

namespace NotificationSystem.Models
{
    internal class EmailNotification : INotificationMode
    {
        public bool CanSend(User user)
        {
            return !string.IsNullOrWhiteSpace(user.Email);
        }

        public void Send(User sender, User receiver, string message)
        {
            Console.WriteLine($"Email Sent Successfully");
        }
    }
}