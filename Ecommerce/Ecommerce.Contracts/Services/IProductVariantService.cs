using Ecommerce.Models.DTOs;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Services
{
    public interface IProductVariantService
    {
        Task<ProductVariantResponse> GetVariantById(int variantId);
        Task<ProductVariantResponse> AddVariant(int productId, AddProductVariantRequest request);
        Task<ProductVariantResponse> UpdateVariant(int variantId, UpdateProductVariantRequest request);
        Task<bool> ArchiveVariant(int variantId);
        Task<ProductImageResponse> AddImage(int variantId, CreateProductImageRequest request);
        Task<bool> DeleteImage(int imageId);
    }
}
