namespace NotificationSystem.Models
{
    internal class Notification
    {
        public enum NotificationType
        {
            Email = 1,
            SMS = 2,
            WhatsApp = 3
        }

        public string Message { get; set; } = string.Empty;
        public DateTime SentDate { get; set; } = DateTime.Now;
        public User Sender { get; set; }
        public User Receiver { get; set; }
        public NotificationType Type { get; set; }

        public Notification(string message, User sender, User receiver, NotificationType type)
        {
            Message = message;
            SentDate = DateTime.Now;
            Sender = sender;
            Receiver = receiver;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Type} Notification\n" +
                   $"  Message : {Message}\n" +
                   $"  Sent At : {SentDate:yyyy-MM-dd HH:mm:ss}\n" +
                   $"  From    : {Sender.Name}\n" +
                   $"  To      : {Receiver.Name}";
        }
    }
}