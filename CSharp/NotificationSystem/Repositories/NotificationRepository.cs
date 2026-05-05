using NotificationSystem.Models;

namespace NotificationSystem.Repositories
{
    internal class NotificationRepository
    {
        List<Notification> notifications = new List<Notification>();

        public void Add(Notification notification)
        {
            notifications.Add(notification);
        }

        //Indexer : return all user notifications wrt username.
        public List<Notification> this[string username]
        {
            get => notifications.Where(n => n.Sender.Name == username || n.Receiver.Name == username).ToList();
        }
    }
}