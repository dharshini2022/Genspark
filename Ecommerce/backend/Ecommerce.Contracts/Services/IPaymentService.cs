using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IPaymentService
    {
        Task<Payment> CreatePendingPayment(decimal total);
        Task UpdatePaymentToPaid(Payment payment, string transactionId);
        Task UpdatePaymentToFailed(Payment payment);
        Task<MakePaymentResponse> MakePayment(int orderId, MakePaymentRequest request);
        Task<string> CreateCheckoutSessionUrl(int orderId);
        Task<ICollection<PaymentResponseDTO>> GetMyPaymentHistory();
        Task<PageResponse<PaymentResponseDTO>> GetOverallPaymentHistory(PageRequest request);
        Task<bool> HandleStripeWebhookEvent(string jsonPayload, string signatureHeader);
    }
}