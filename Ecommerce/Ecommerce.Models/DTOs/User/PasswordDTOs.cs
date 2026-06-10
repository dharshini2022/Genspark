using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old Password is required")]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "New Password is required")]
        public string NewPassword { get; set; } = null!;
    }
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = null!;
    }
}