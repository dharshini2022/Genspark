using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IProductRepository : IRepository<int, Product>
    {
        Task<Product?> GetById(int id);
        Task<int> GetProductsCount(int? categoryId, decimal? minPrice = null, decimal? maxPrice = null, bool onlyDiscounted = false, string? searchQuery = null);
        Task<ICollection<Product>> GetProductsPaged(int pageNumber, int pageSize, string? sortBy, string? sortOrder, int? categoryId, decimal? minPrice = null, decimal? maxPrice = null, string? SearchQuery = null);
        Task<ICollection<Product>> SearchProductsByName(string query);
        Task<ICollection<Product>> GetProductsByVendorId(int vendorId);
        Task<ICollection<Product>> GetActiveProducts();
    }
}
