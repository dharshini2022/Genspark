namespace BankingAPI.Models.DTOs
{
    public class CreateAccountRequest
    {
        public float Balance { get; set; }
        public string? AccountType { get; set; } = "Saving Account";

        public int CustomerId { get; set; }

    }
}
