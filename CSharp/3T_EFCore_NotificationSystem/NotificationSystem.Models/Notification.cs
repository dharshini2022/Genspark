
namespace NotificationSystem.Models
{
    public class Notification
    {
        public enum NotificationType { Email = 1, SMS = 2, WhatsApp = 3 }

        public int Id;
        public string Message { get; set; } = string.Empty;
        public DateTime SentDate { get; set; } = DateTime.Now;
        public int SenderId { get; set; }
        public int ReceiverId { get; set; } 
        public NotificationType Type { get; set; }
        public string SenderContact { get; set; }  
        public string ReceiverContact { get; set; } 

        public Notification(){}

        public Notification(string Message, int SenderId, int ReceiverId, NotificationType Type, string SenderContact, string ReceiverContact)
        {
            this.Message = Message;
            SentDate = DateTime.Now;
            this.SenderId = SenderId;
            this.ReceiverId = ReceiverId;
            this.Type = Type;
            this.SenderContact = SenderContact;
            this.ReceiverContact = ReceiverContact;
        }

        public enum DeliveryStatus { Sent = 1, Received = 2}
        public virtual User Sender {get; set;} = null;
        public virtual User Receiver {get; set;} = null;
        public override string ToString()
        {
            string label = Type == NotificationType.Email ? "Email" : "Phone";
            return $"  {Type} {SentDate:yyyy-MM-dd HH:mm:ss}\n From : {SenderContact}\n To   : {ReceiverContact}\n Msg  : {Message}";
        }

    }
}