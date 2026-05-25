namespace BankingAPI.Models.DTOs
{
    public class CreateAccountRequest
    {
        public float Balance { get; set; }
        public string? AccountType { get; set; } = string.Empty;

        public int CustomerId { get; set; }

    }
}
