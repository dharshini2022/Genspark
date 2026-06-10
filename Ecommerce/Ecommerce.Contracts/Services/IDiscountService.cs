using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IDiscountService
    {
        Task<DiscountResponse> CreateDiscount(CreateDiscountRequest request);
        Task<ICollection<DiscountResponse>> GetActiveDiscounts(PageRequest request);
        Task<ICollection<DiscountResponse>> GetDiscountsOfProduct(int productId, int categoryId, int vendorId);
        Task<ICollection<DiscountResponse>> GetVendorDiscounts(int vendorId);
        Task<ICollection<DiscountResponse>> GetAllDiscounts();
        Task<ToggleDiscountStatusResponse> DeactivateDiscount(string Code);
        Task<ICollection<DiscountCartResponse>> EvaluateCartDiscounts(CartEvaluationRequest request);
    }
}
