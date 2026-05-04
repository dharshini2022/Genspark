using NotificationSystem.Models;

namespace NotificationSystem.Interfaces
{
    internal interface INotificationMode
    {
        bool CanSend(User user);
        void Send(User sender, User receiver, string message);
    }
}