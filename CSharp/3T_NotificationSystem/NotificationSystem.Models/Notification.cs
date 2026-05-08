
namespace NotificationSystem.Models
{
    public class Notification
    {
        public enum NotificationType { Email = 1, SMS = 2, WhatsApp = 3 }
        public enum DeliveryStatus { Sent = 1, Received = 2}

        public string Message { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public User Sender { get; set; }
        public User Receiver { get; set; } 
        public NotificationType Type { get; set; }
        public string SenderContact { get; set; }  
        public string ReceiverContact { get; set; } 

        public Notification(string Message, User Sender, User Receiver, NotificationType Type, string SenderContact, string ReceiverContact)
        {
            this.Message = Message;
            SentDate = DateTime.Now;
            this.Sender = Sender;
            this.Receiver = Receiver;
            this.Type = Type;
            this.SenderContact = SenderContact;
            this.ReceiverContact = ReceiverContact;
        }

        public override string ToString()
        {
            string label = Type == NotificationType.Email ? "Email" : "Phone";
            return $"  {Type} {SentDate:yyyy-MM-dd HH:mm:ss}\n" +
                   $"  From : {Sender.Name} ({label}: {SenderContact})\n" +
                   $"  To   : {Receiver.Name} ({label}: {ReceiverContact})\n" +
                   $"  Msg  : {Message}";
        }
    }
}