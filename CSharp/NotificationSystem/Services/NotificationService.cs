using NotificationSystem.Interfaces;
using NotificationSystem.Models;
using NotificationSystem.Repositories;

namespace NotificationSystem.Services
{
    internal class NotificationService
    {
        private readonly NotificationRepository _repo = new();

        public void Send(INotification mode, User sender, User receiver, string message)
        {
            Notification? notification = mode.Send(sender, receiver, message);
            if (notification != null)
                _repo.Add(notification);
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
            }
        }
    }
}