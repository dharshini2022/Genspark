using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace Ecommerce.BLL
{
    public class StripeService : IStripeService
    {
        private readonly string _clientBaseUrl;

        private readonly string _webhookSecret;

        public StripeService(IConfiguration configuration)
        {
            var secretKey = configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
            StripeConfiguration.ApiKey = secretKey;
            _clientBaseUrl = configuration["Stripe:ClientBaseUrl"] ?? "http://localhost:4200";
            _webhookSecret = configuration["Stripe:WebhookSecret"] ?? "";
        }

        public Event ConstructEvent(string json, string signatureHeader)
        {
            return EventUtility.ConstructEvent(json, signatureHeader, _webhookSecret);
        }



        /// <summary>
        /// Creates a Stripe-hosted Checkout Session.
        /// Returns the session URL to redirect the customer to.
        /// Stripe handles all UI — card, UPI, wallets, etc.
        /// </summary>
        public async Task<string> CreateCheckoutSession(decimal amountInRupees, string currency, string orderId, string productName, string customerEmail)
        {
            var amountInPaise = (long)(amountInRupees * 100);

            var options = new SessionCreateOptions
            {
                CustomerEmail = customerEmail,
                PaymentMethodTypes = new List<string> { "card", "upi" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency.ToLower(),
                            UnitAmount = amountInPaise,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = productName,
                                Description = $"Order #{orderId} — BBS Ecommerce"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{_clientBaseUrl}/customer-home/payment-status?orderId={orderId}&session_id={{CHECKOUT_SESSION_ID}}&status=success",
                CancelUrl  = $"{_clientBaseUrl}/customer-home/payment-status?orderId={orderId}&status=cancelled",
                Metadata = new Dictionary<string, string>
                {
                    ["order_id"] = orderId,
                    ["platform"] = "Ecommerce"
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }
    }
}
