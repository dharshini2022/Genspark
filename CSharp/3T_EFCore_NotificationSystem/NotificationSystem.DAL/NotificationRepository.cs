using Microsoft.EntityFrameworkCore;
using System.Linq;
using NotificationSystem.Models;
using NotificationSystem.DAL.Contexts;

namespace NotificationSystem.DAL
{
    public class NotificationRepository
    {
        private NotificationDbContext _dbContext;
        public NotificationRepository()
        {
            _dbContext = new NotificationDbContext();
        }

        public void SaveNotification(Notification notification)
        {
            try
            {
                _dbContext.notifications.Add(notification);

                int rowsAffected = _dbContext.SaveChanges();
                if(rowsAffected == 0)
                {
                    Console.WriteLine("Notification Save Failed");
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        public List<string>? GetNotificationByUsername(string name)
        {
            try
            {
                var notificationList = _dbContext.notifications
                        .Include(n => n.Sender)
                        .Include(n => n.Receiver)
                        .Where(n => n.Sender.Name == name || n.Receiver.Name == name )
                        .OrderByDescending(n => n.SentDate)
                        .Select(n => $"Notification Date: {n.SentDate}\nFrom: {n.Sender.Name}\nTo: {n.Receiver.Name}\nMsg: {n.Message}")
                        .ToList();
                return notificationList;

            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
    }
}