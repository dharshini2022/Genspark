namespace NotificationSystem.Models
{

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool HasWhatsapp { get; set; } = false;

        public User(){}

        public User(int Id,string Name, string Email, string Phone, bool HasWhatsapp = false)
        {
            this.Id = Id;
            this.Name = Name;
            this.Email = Email;
            this.Phone = Phone;         
            this.HasWhatsapp = HasWhatsapp;
        }

        public ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();

        public ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();

        public override string ToString()
        {
            string whatsappStatus = HasWhatsapp ? "Active" : "Inactive";
            return $"  Name         : {Name}\n" +
                   $"  Email        : {Email}\n" +
                   $"  Phone        : {Phone}\n" +
                   $"  WhatsApp     : {whatsappStatus}";
        }

    }
}