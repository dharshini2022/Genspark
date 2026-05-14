using NotificationSystem.Models;

namespace NotificationSystem.BLL.Interfaces
{
    public interface INotificationSender
    {
        bool CanSend(User sender, User receiver);
        Notification? Send(User sender, User receiver, string message);
    }
}