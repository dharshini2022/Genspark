namespace BankingAPI.Models.DTOs
{
    public class DepositRequest
    {
        public string ToAccountNumber { get; set; } = string.Empty;
        public float Amount { get; set; }
    }
}