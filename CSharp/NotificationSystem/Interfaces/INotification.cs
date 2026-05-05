// Interfaces/INotification.cs
using NotificationSystem.Models;

namespace NotificationSystem.Interfaces
{
    internal interface INotification
    {
        bool CanSend(User sender, User receiver);
        Notification? Send(User sender, User receiver, string message);
    }
}