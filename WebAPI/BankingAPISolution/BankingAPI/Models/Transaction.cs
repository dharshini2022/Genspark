namespace BankingAPI.Models
{
    public enum TransactionStatus
    {
        Success = 1,
        Failed = 2
    }
    public class Transaction
    {
        public int TransactionReferenceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? FromAccountNumber { get; set; }
        public string? ToAccountNumber { get; set; }
        public float Amount {get; set; }
        public string? Reason {get;set; }
        public TransactionStatus Status { get; set; }

        public Account? FromAccount { get; set; }
        public Account? ToAccount { get; set; }
    }
}