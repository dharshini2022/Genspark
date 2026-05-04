namespace NotificationSystem.Models
{

    internal class User
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool HasWhatsapp { get; set; } = false;

        public User(string name, string email, string phone, bool hasWhatsapp = false)
        {
            Name = name;
            Email = email;
            Phone = phone;         
            HasWhatsapp = hasWhatsapp;
        }

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