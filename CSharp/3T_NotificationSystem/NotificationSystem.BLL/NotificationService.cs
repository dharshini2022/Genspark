using NotificationSystem.DAL.Interfaces;
using NotificationSystem.Models;
using NotificationSystem.DAL;
using NotificationSystem.BLL.Interfaces;
using NotificationSystem.Exceptions;

namespace NotificationSystem.BLL
{
    public class NotificationService
    {
        private readonly NotificationRepository _repo = new();

        public void Send(INotificationSender mode, User sender, User receiver, string message)
        {
            if( sender == null || receiver == null )
            {
                throw new ContactNotFoundException($"User Not Found");
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new InvalidMessageException("Messsage cannot be empty");
            }
            if(message.Trim().Length < 5)
            {
                throw new InvalidMessageException("Message must contain minimum 5 characters");
            }
            if(mode.GetType().Name == "SMSNotificationSender" && message.Trim().Length > 160)
            {
                throw new InvalidMessageException("SMS Message can't exceed 160 characters");
            }
            Notification? notification = mode.Send(sender, receiver, message);
            if(notification == null)
            {
                throw new Exception("Failed to Send Notification");
            }
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