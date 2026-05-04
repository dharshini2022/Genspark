using NotificationSystem.Interfaces;
using NotificationSystem.Models;

namespace NotificationSystem.Services
{

    internal class NotificationService
    {

        public void Send(INotificationMode mode, User sender, User receiver, string message, Notification.NotificationType type)
        {
            if (!mode.CanSend(receiver))
            {
                Console.WriteLine("Cannot send notification — receiver is missing required contact info.");
                return;
            }

            mode.Send(sender,receiver, message);
            var notification = new Notification(message, sender, receiver, type);
            Console.WriteLine(notification);
        }

    }
}