using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class RefreshRequest
    {
        [Required(ErrorMessage = "Expired access token is required")]
        public string ExpiredAccessToken { get; set; } = null!;

        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = null!;
    }
}
