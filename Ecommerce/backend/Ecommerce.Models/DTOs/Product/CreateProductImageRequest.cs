using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{
    public class CreateProductImageRequest
    {
        [Required(ErrorMessage = "Image URL is required.")]
        public string ImageUrl { get; set; } = null!;
        
        [Range(0, int.MaxValue, ErrorMessage = "Image order cannot be negative.")]
        public int ImageOrder { get; set; }
    }
}