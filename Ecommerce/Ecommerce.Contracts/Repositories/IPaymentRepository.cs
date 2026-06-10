using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IPaymentRepository : IRepository<int, Payment>
    {
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
        Task<ICollection<Payment>> GetPaymentHistoryByUserIdAsync(int userId);
        Task<ICollection<Payment>> GetPaymentsPagedAsync(int pageNumber, int pageSize, string? statusSort);
    }
}
