using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Stripe;

namespace Ecommerce.Contracts.Services
{
    public interface IStripeService
    {
        Task<string> CreateCheckoutSession(decimal amountInRupees, string currency, string orderId, string productName, string customerEmail);
        Event ConstructEvent(string json, string signatureHeader);
    }
}
