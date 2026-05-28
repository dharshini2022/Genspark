using System.ComponentModel.DataAnnotations;

namespace BankingAPI.Models.DTOs
{
    public class CreateAccountRequest
    {
        public float Balance { get; set; }

        [Required(ErrorMessage = "Account type is required(Saving Account or Current Account)")]
        public string? AccountType { get; set; } = "Saving Account";

        [Required(ErrorMessage = "CustomerId is required")]
        public int CustomerId { get; set; }

    }
}
