using Ecommerce.Models.DTOs;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Services
{
    public interface IProductService
    {
        Task<PageResponse<ProductResponse>> GetProductsCatalog(ProductFilterRequest query);
        Task<ICollection<ProductSearchResult>> SearchProducts(string searchTerm);
        Task<ProductResponse> GetProductDetails(int productId);

        Task<ICollection<ProductResponse>> GetVendorProducts();
        Task<ICollection<ProductResponse>> GetProductsByVendorId(int vendorId);
        Task<ProductResponse> CreateProduct(CreateProductRequest request);
        Task<ProductResponse> UpdateProduct(int productId, UpdateProductRequest request);
        Task<ProductResponse> ToggleProductStatus(int productId);
        Task<ProductResponse?> PublishProduct(int productId);
    }
}
