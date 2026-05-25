namespace BankingAPI.Models.DTOs
{
    public class TransactionResponse
    {
        public int TransactionReferenceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string FromAccountNumber { get; set; } = string.Empty;
        public string ToAccountNumber { get; set; } = string.Empty;
        public float Amount { get; set; }
        public string Reason {get; set;} = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}