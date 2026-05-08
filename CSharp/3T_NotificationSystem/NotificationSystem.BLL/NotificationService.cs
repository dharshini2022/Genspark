using NotificationSystem.DAL.Interfaces;
using NotificationSystem.Models;
using NotificationSystem.DAL;
using NotificationSystem.BLL.Interfaces;

namespace NotificationSystem.BLL
{
    public class NotificationService
    {
        private readonly NotificationRepository _repo = new();

        public void Send(INotificationSender mode, User sender, User receiver, string message)
        {
            Notification? notification = mode.Send(sender, receiver, message);
            if (notification != null)
                _repo.SaveNotification(notification);
        }

        public void PrintByUsername(User user)
        {
            Console.WriteLine("\nUser Details");
            Console.WriteLine(user);

            //call of indexed data
            var notifications = _repo[user.Name];
            if (notifications.Count == 0)
            {
                Console.WriteLine("\n  No notifications sent by this user.");
                return;
            }

            Console.WriteLine($"\nNotifications Sent by {user.Name}");
            foreach (var n in notifications)
            {
                Console.WriteLine(n);
                Console.WriteLine("---------------------------");
            }
        }

        
    }
}