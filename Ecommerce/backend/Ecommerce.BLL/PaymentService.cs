using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.BLL
{
    public class PaymentService: IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly AppDbContext _dbContext;
        private readonly ICurrentUserService _currentUser;
        private readonly IOrderRepository _orderRepository;
        private readonly IStripeService _stripeService;
        private readonly IShipmentService _shipmentService;
        private readonly IVendorSettlementService _vendorSettlementService;
        private readonly IProductVariantService _productVariantService;
        private readonly IDiscountService _discountService;
        private readonly ICartRepository _cartRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IBackgroundJobScheduler _backgroundJobScheduler;
        public TimeSpan DeliveryScheduleDelay { get; set; } = TimeSpan.FromMinutes(2);

        public PaymentService(
            IPaymentRepository paymentRepository,
            AppDbContext dbContext,
            ICurrentUserService currentUser,
            IOrderRepository orderRepository,
            IStripeService stripeService,
            IShipmentService shipmentService,
            IVendorSettlementService vendorSettlementService,
            IProductVariantService productVariantService,
            IDiscountService discountService,
            ICartRepository cartRepository,
            IServiceProvider serviceProvider,
            IMapper mapper,
            IEmailService emailService,
            IBackgroundJobScheduler backgroundJobScheduler)
        {
            _paymentRepository = paymentRepository;
            _dbContext = dbContext;
            _currentUser = currentUser;
            _orderRepository = orderRepository;
            _stripeService = stripeService;
            _shipmentService = shipmentService;
            _vendorSettlementService = vendorSettlementService;
            _productVariantService = productVariantService;
            _discountService = discountService;
            _cartRepository = cartRepository;
            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _emailService = emailService;
            _backgroundJobScheduler = backgroundJobScheduler;
        }

        public async Task<Payment> CreatePendingPayment(decimal total)
        {
            var payment = new Payment
            {
                Amount = total,
                TransactionId = $"PENDING-{Guid.NewGuid()}",
                Provider = "Stripe",
                Status = PaymentStatus.Pending,
                PaidAt = DateTime.Now
            };

            var result = await _paymentRepository.Create(payment);
            return result;
        }

        public async Task UpdatePaymentToPaid(Payment payment, string transactionId)
        {
            payment.Status = PaymentStatus.Paid;
            payment.TransactionId = transactionId;
            payment.StripePaymentIntentId = transactionId;
            payment.PaidAt = DateTime.Now;
            await _paymentRepository.Update(payment.Id, payment);
        }

        public async Task UpdatePaymentToFailed(Payment payment)
        {
            payment.Status = PaymentStatus.Failed;
            await _paymentRepository.Update(payment.Id, payment);
        }

        public async Task<MakePaymentResponse> MakePayment(int orderId, MakePaymentRequest request)
        {
            int userId = _currentUser.UserId;
            var order = await ValidateOrderForPayment(orderId);
            try
            {
                if (request.PaymentMethodId != "stripe_checkout")
                {
                    throw new ArgumentException("Invalid payment method.");
                }

                string transactionId = $"ch_checkout_{Guid.NewGuid().ToString("N").Substring(0, 16)}";
                await ConfirmOrder(order, transactionId);
                ScheduleDelivery(orderId);
                
                _backgroundJobScheduler.ScheduleOrderConfirmationNotifications(orderId);

                return BuildPaymentResponse(true, orderId, OrderStatus.Confirmed.ToString(), "Payment successful. Your order is confirmed!", transactionId);
            }
            catch (Exception ex)
            {
                await FailOrderAsync(order);
                return BuildPaymentResponse(false, orderId, OrderStatus.PaymentFailed.ToString(), $"Payment failed: {ex.Message}", null);
            }
        }

        public async Task<string> CreateCheckoutSessionUrl(int orderId)
        {
            var order = await ValidateOrderForPayment(orderId);
            var productName = order.Items?.FirstOrDefault()?.Variant?.Product?.Name ?? "Your Order";
            var customerEmail = order.User?.Email ?? _currentUser.Email ?? "";
            var url = await _stripeService.CreateCheckoutSession(order.Total, "inr", orderId.ToString(), productName, customerEmail);
            return url;
        }

        private async Task<Order> ValidateOrderForPayment(int orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsById(orderId)   ?? throw new KeyNotFoundException($"Order {orderId} not found.");
            if (order.UserId != _currentUser.UserId)    throw new UnauthorizedAccessException("This order does not belong to you.");
            if (order.Status != OrderStatus.PendingPayment)throw new InvalidOperationException($"Order is not in PendingPayment state. Current status: {order.Status}.");

            return order;
        }

        private static MakePaymentResponse BuildPaymentResponse(bool success, int orderId, string status, string message, string? chargeId)
        {
            return new MakePaymentResponse
            {
                Success = success,
                OrderId = orderId,
                Status = status,
                Message = message,
                ChargeId = chargeId
            };
        }

        private async Task UpdateOrderPaymentStatus(Order order, string chargeId)
        {
            order.Status = OrderStatus.Confirmed;
            order.OrderPaymentStatus = PaymentStatus.Paid;
            order.StripePaymentIntentId = chargeId;
            await _orderRepository.Update(order.Id, order);
        }

        private async Task InitiateShipment(Order order)
        {
            var shipmentIds = order.Items
                .Where(i => i.ShipmentId.HasValue)
                .Select(i => i.ShipmentId!.Value)
                .Distinct()
                .ToList();
            foreach (var shipmentId in shipmentIds)
            {
                await _shipmentService.UpdateShipmentStatus(shipmentId, ShipmentStatus.Initiated);
            }
        }

        private async Task ConfirmOrder(Order order, string chargeId)
        {
            await using var orderTransaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await UpdatePaymentToPaid(order.Payment, chargeId);
                await UpdateOrderPaymentStatus(order, chargeId);
                await InitiateShipment(order);
                await _vendorSettlementService.CreateSettlementsForOrder(order, chargeId, order.Discount);
                
                await _productVariantService.ConfirmStockReservation(order.Id);
                await _discountService.ConfirmDiscountReservation(order.Id);

                var cart = await _cartRepository.GetCartByUserId(order.UserId);
                if (cart != null)
                {
                    await _cartRepository.ClearCart(cart.Id);
                }

                await orderTransaction.CommitAsync();
            }
            catch(Exception)
            {
                await orderTransaction.RollbackAsync();
                throw;
            }
        }

        private async Task FailOrderAsync(Order order)
        {
            await using var orderTransaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                order.Status = OrderStatus.PaymentFailed;
                order.OrderPaymentStatus = PaymentStatus.Failed;
                await _orderRepository.Update(order.Id, order);

                await UpdatePaymentToFailed(order.Payment);

                await _productVariantService.ReleaseStockReservation(order.Id);
                await _discountService.ReleaseDiscountReservation(order.Id);

                await orderTransaction.CommitAsync();
            }
            catch
            {
                await orderTransaction.RollbackAsync();
                throw;
            }
        }

        private void ScheduleDelivery(int orderId)
        {
            _backgroundJobScheduler.ScheduleDelivery(orderId, DeliveryScheduleDelay);
        }

        public async Task<ICollection<PaymentResponseDTO>> GetMyPaymentHistory()
        {
            var payments = await _paymentRepository.GetPaymentHistoryByUserIdAsync(_currentUser.UserId);
            return _mapper.Map<ICollection<PaymentResponseDTO>>(payments);
        }

        public async Task<PageResponse<PaymentResponseDTO>> GetOverallPaymentHistory(PageRequest request)
        {
            var (pagedPayments, totalCount) = await _paymentRepository.GetPagedPaymentsWithDetails(request.SearchTerm, request.PageNumber, request.PageSize);

            var items = _mapper.Map<List<PaymentResponseDTO>>(pagedPayments);

            return new PageResponse<PaymentResponseDTO>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<bool> HandleStripeWebhookEvent(string jsonPayload, string signatureHeader)
        {
            var stripeEvent = _stripeService.ConstructEvent(jsonPayload, signatureHeader);
            if (stripeEvent == null) return false;

            if (stripeEvent.Type == Stripe.EventTypes.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
                if (paymentIntent != null && paymentIntent.Metadata.TryGetValue("order_id", out var orderIdStr) && int.TryParse(orderIdStr, out var orderId))
                {
                    var order = await _orderRepository.GetOrderWithDetailsById(orderId);
                    if (order != null && order.Status == OrderStatus.PendingPayment)
                    {
                        await ConfirmOrder(order, paymentIntent.Id);
                        ScheduleDelivery(orderId);
                        _backgroundJobScheduler.ScheduleOrderConfirmationNotifications(orderId);
                    }
                }
            }
            else if (stripeEvent.Type == Stripe.EventTypes.PaymentIntentPaymentFailed)
            {
                var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
                if (paymentIntent != null && paymentIntent.Metadata.TryGetValue("order_id", out var orderIdStr) && int.TryParse(orderIdStr, out var orderId))
                {
                    var order = await _orderRepository.GetOrderWithDetailsById(orderId);
                    if (order != null && order.Status == OrderStatus.PendingPayment)
                    {
                        await FailOrderAsync(order);
                    }
                }
            }
            return true;
        }
    }
}