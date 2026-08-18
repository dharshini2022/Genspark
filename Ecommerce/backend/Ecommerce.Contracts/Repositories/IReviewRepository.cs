using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IReviewRepository : IRepository<int, Review>
    {
        Task<ICollection<Review>> GetReviewsByProductId(int productId);
        Task<ICollection<Review>> GetReviewsByUserId(int userId);
        Task<ICollection<Review>> GetReviewsByVendorId(int vendorId);
        Task<Review?> GetReviewWithDetailsById(int reviewId);
        Task<ICollection<Review>> GetAllReviewsWithDetails();
        Task<Review?> GetReviewByUserAndProduct(int userId, int productId);
    }
}
