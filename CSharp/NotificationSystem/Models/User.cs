namespace NotificationSystem.Models
{

    internal class User : IComparable<User>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool HasWhatsapp { get; set; } = false;

        public User(string Name, string Email, string Phone, bool HasWhatsapp = false)
        {
            this.Name = Name;
            this.Email = Email;
            this.Phone = Phone;         
            this.HasWhatsapp = HasWhatsapp;
        }

        public override string ToString()
        {
            string whatsappStatus = HasWhatsapp ? "Active" : "Inactive";
            return $"  Name         : {Name}\n" +
                   $"  Email        : {Email}\n" +
                   $"  Phone        : {Phone}\n" +
                   $"  WhatsApp     : {whatsappStatus}";
        }

         public int CompareTo(User? other)
        {
            if(other == null)
            {
                return 0;
            }
            return this.AccountNumber.CompareTo(other.Name);
        }

    }
}