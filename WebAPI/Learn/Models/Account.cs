namespace Learn.Models
{
    public class Account
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string Name {get; set;}  = string.Empty;
        public decimal Balance { get; set; } = 0.0M;
        public DateTime OpeninigDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = string.Empty;
    }
}
