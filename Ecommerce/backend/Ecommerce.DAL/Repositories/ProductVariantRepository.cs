using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Ecommerce.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DAL.Repositories
{
    public class ProductVariantRepository : AbstractRepository<int, ProductVariant>, IProductVariantRepository
    {
        private readonly AppDbContext _dbContext;

        public ProductVariantRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<ProductVariant?> GetById(int id)
        {
            return await _dbContext.ProductVariants
                .Include(v => v.VariantImages)
                .Include(v => v.OrderItems)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<ICollection<ProductVariant>> GetVariantsByProductId(int productId)
        {
            return await _dbContext.ProductVariants
                .Include(v => v.VariantImages)
                .Include(v => v.OrderItems)
                .Where(v => v.ProductId == productId && v.IsActive)
                .ToListAsync();
        }

        public async Task<ProductVariant?> GetDefaultVariant(int productId)
        {
            return await _dbContext.ProductVariants
                .Include(v => v.VariantImages)
                .Include(v => v.OrderItems)
                .FirstOrDefaultAsync(v => v.ProductId == productId && v.IsDefault && v.IsActive);
        }

        public async Task<bool> IncreaseStock(int variantId, int quantity)
        {
            var variant = await GetById(variantId) ?? throw new InvalidProductException("Product Variant Not Found");
            variant.StockQty += quantity;
            await Update(variantId, variant);
            return true;
        }

        public async Task<bool> DecreaseStock(int variantId, int quantity)
        {
            var variant = await GetById(variantId) ?? throw new InvalidProductException("Product Variant Not Found");
            variant.StockQty -= quantity;
            await Update(variantId, variant);
            return true;
        }

        public async Task<bool> ReserveStockAtomic(int variantId, int quantity)
        {
            var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(
                @"UPDATE product_variants 
                  SET ""ReservedStockQty"" = ""ReservedStockQty"" + {0} 
                  WHERE ""Id"" = {1} AND (""StockQty"" - ""ReservedStockQty"") >= {0}",
                quantity, variantId);

            return rowsAffected > 0;
        }
    }
}
