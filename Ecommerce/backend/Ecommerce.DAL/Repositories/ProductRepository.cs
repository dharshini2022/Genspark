using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class ProductRepository : AbstractRepository<int, Product>, IProductRepository
    {
        private readonly AppDbContext _dbContext;

        public ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<Product?> GetById(int id)
        {
            return await _dbContext.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages.OrderBy(img => img.ImageOrder))
                .Include(p => p.Variants)
                    .ThenInclude(v => v.OrderItems)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<int> GetProductsCount(int? categoryId, decimal? minPrice = null, decimal? maxPrice = null, bool onlyDiscounted = false, string? searchQuery = null)
        {
            var query = _dbContext.Products
                .Where(p => p.Status == ProductStatus.Active && p.Variants.Any(v => v.IsDefault))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = ApplySearchFilter(query, searchQuery);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Variants.Any(v => v.IsActive && v.Price >= minPrice.Value));
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Variants.Any(v => v.IsActive && v.Price <= maxPrice.Value));
            }

            if (onlyDiscounted)
            {
                query = query.Where(p => _dbContext.Discounts.Any(d =>
                    d.IsActive && d.ExpiresAt > DateTime.Now &&
                    (
                        (d.Scope == DiscountScope.Product && d.ProductId == p.Id) ||
                        (d.Scope == DiscountScope.Category && d.CategoryId == p.CategoryId) ||
                        (d.Scope == DiscountScope.Vendor && d.VendorId == p.VendorId)
                    )
                ));
            }

            return await query.CountAsync();
        }

        public async Task<ICollection<Product>> GetProductsPaged(int pageNumber, int pageSize, string? sortBy, string? sortOrder, int? categoryId, decimal? minPrice = null, decimal? maxPrice = null, string? SearchQuery = null)
        {
            var query = _dbContext.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Variants.Where(v => v.IsDefault))
                    .ThenInclude(v => v.VariantImages.Where(img => img.ImageOrder == 1))
                .Include(p => p.Variants.Where(v => v.IsDefault))
                    .ThenInclude(v => v.OrderItems)
                .Where(p => p.Status == ProductStatus.Active && p.Variants.Any(v => v.IsDefault))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = ApplySearchFilter(query, SearchQuery);
            }

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Variants.Any(v => v.IsActive && v.Price >= minPrice.Value));

            if (maxPrice.HasValue)
                query = query.Where(p => p.Variants.Any(v => v.IsActive && v.Price <= maxPrice.Value));

            if (string.Equals(sortBy, "discount", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => _dbContext.Discounts.Any(d =>
                    d.IsActive && d.ExpiresAt > DateTime.Now &&
                    (
                        (d.Scope == DiscountScope.Product && d.ProductId == p.Id) ||
                        (d.Scope == DiscountScope.Category && d.CategoryId == p.CategoryId) ||
                        (d.Scope == DiscountScope.Vendor && d.VendorId == p.VendorId)
                    )
                ));
            }

            bool isDesc = string.Equals(sortOrder?.Trim().ToLower(), "desc");

            if (!string.Equals(sortBy, "discount", StringComparison.OrdinalIgnoreCase))
            {
                query = sortBy?.Trim().ToLower() switch
                {
                    "price" => isDesc
                        ? query.OrderByDescending(p => p.Variants.Where(v => v.IsActive).Min(v => v.Price))
                        : query.OrderBy(p => p.Variants.Where(v => v.IsActive).Min(v => v.Price)),

                    "newest" => isDesc
                        ? query.OrderByDescending(p => p.CreatedAt)
                        : query.OrderBy(p => p.CreatedAt),

                    "rating" => isDesc
                        ? query.OrderByDescending(p => p.Rating)
                        : query.OrderBy(p => p.Rating),

                    _ => query.OrderByDescending(p => p.CreatedAt)   
                };
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

    
        public async Task<ICollection<Product>> SearchProductsByName(string query)
        {
            var dbQuery = _dbContext.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.OrderItems)
                .Where(p => p.Status == ProductStatus.Active)
                .AsQueryable();

            dbQuery = ApplySearchFilter(dbQuery, query);

            return await dbQuery.ToListAsync();
        }

        private static string StemWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return word;
            
            if (word.EndsWith("ies", StringComparison.OrdinalIgnoreCase) && word.Length > 3)
            {
                return word.Substring(0, word.Length - 3) + "y";
            }
            if (word.EndsWith("es", StringComparison.OrdinalIgnoreCase) && word.Length > 2)
            {
                return word.Substring(0, word.Length - 2);
            }
            if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase) && !word.EndsWith("ss", StringComparison.OrdinalIgnoreCase) && word.Length > 1)
            {
                return word.Substring(0, word.Length - 1);
            }
            return word;
        }

        private static IQueryable<Product> ApplySearchFilter(IQueryable<Product> query, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery)) return query;

            var words = searchQuery.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                var stemmed = StemWord(word);
                if (stemmed != word)
                {
                    query = query.Where(p => p.Name.ToLower().Contains(word) || 
                                             p.Name.ToLower().Contains(stemmed) || 
                                             (p.Category != null && (p.Category.Name.ToLower().Contains(word) || p.Category.Name.ToLower().Contains(stemmed))));
                }
                else
                {
                    query = query.Where(p => p.Name.ToLower().Contains(word) || 
                                             (p.Category != null && p.Category.Name.ToLower().Contains(word)));
                }
            }
            return query;
        }

       
        public async Task<ICollection<Product>> GetProductsByVendorId(int vendorId)
        {
            return await _dbContext.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.OrderItems)
                .Where(p => p.VendorId == vendorId)
                .ToListAsync();
        }


        public async Task<ICollection<Product>> GetActiveProducts()
        {
            return await _dbContext.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.OrderItems)
                .Where(p => p.Status == ProductStatus.Active)
                .ToListAsync();
        }
    }
}
