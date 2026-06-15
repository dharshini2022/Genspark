using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.BLL
{
    public class OrderSummary{

        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal Total { get; set; }

    }
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _dbContext;
        private readonly IOrderRepository _orderRepository;
        private readonly IDiscountRepository _discountRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;
        private readonly IPaymentService _paymentService;
        private readonly IDiscountService _discountService;
        private readonly IShipmentService _shipmentService;
        private readonly IProductVariantService _productVariantService;
        private readonly IServiceProvider _serviceProvider;

        public const decimal PlatformCommissionRate = 0.05m;  
        public const decimal VendorShippingRate = 0.02m;     
        private const decimal TaxRate = 0.05m;                 
        public TimeSpan StockReleaseDelay { get; set; } = TimeSpan.FromMinutes(10);

        public OrderService(
            AppDbContext dbContext,
            IOrderRepository orderRepository,
            IDiscountRepository discountRepository,
            IVendorRepository vendorRepository,
            ICartRepository cartRepository,
            IOrderItemRepository orderItemRepository,
            IUserRepository userRepository,
            IDiscountService discountService,
            ICartService cartService,
            IPaymentService paymentService,
            ICurrentUserService currentUser,
            IMapper mapper,
            IShipmentService shipmentService,
            IProductVariantService productVariantService,
            IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _orderRepository = orderRepository;
            _discountRepository = discountRepository;
            _vendorRepository = vendorRepository;
            _cartRepository = cartRepository;
            _orderItemRepository = orderItemRepository;
            _userRepository = userRepository;

            _discountService = discountService;
            _cartService = cartService;
            _paymentService = paymentService;
            _shipmentService = shipmentService;
            _productVariantService = productVariantService;

            _serviceProvider = serviceProvider;            
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<OrderResponse> CreateOrder(CreateOrderRequest request)
        {
            int userId = _currentUser.UserId;

            var addresses = await _userRepository.GetAllAddressByUserId(userId);
            var address = addresses.FirstOrDefault(a => a.Id == request.UserAddressId);
            if (address == null)
            {
                throw new KeyNotFoundException("Shipping address not found or does not belong to the user.");
            }

            int initialCartItemCount = (await _cartRepository.GetCartByUserId(userId))?.Items.Count ?? 0;
            var eligibleItems = await _cartService.GetEligibleItems(userId);

            var summary = new OrderSummary();
            summary.Subtotal =eligibleItems.Sum(ci => ci.Variant.Price * ci.Quantity);

            Discount? discount = null;
            if (!string.IsNullOrWhiteSpace(request.DiscountCode))
            {
                discount =await _discountService.ValidateDiscount(request.DiscountCode,eligibleItems,summary.Subtotal);
                summary.DiscountAmount =await _discountService.CalculateDiscountAmount(discount,summary.Subtotal);
            }

            summary.ShippingAmount =CalculateShipping(eligibleItems);
            summary.TaxAmount =CalculateTax(summary.Subtotal,summary.DiscountAmount);
            summary.PlatformCommission = 20.00m;
            summary.Total = CalculateTotal(summary);

            using var orderTransaction =await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var payment =await _paymentService.CreatePendingPayment(summary.Total);
                var order = await CreateInitialOrder(userId,payment,discount,summary,request.UserAddressId);
                var shipmentsMap = await CreateOrderShipments(order.Id, request.UserAddressId, eligibleItems);
                await CreateOrderItems(order.Id, eligibleItems, shipmentsMap);

                foreach (var item in eligibleItems)
                {
                    await _productVariantService.ReserveStock(order.Id, item.VariantId, item.Quantity);
                }

                await orderTransaction.CommitAsync();

                ScheduleStockRelease(order.Id);

                return BuildResponse(order, initialCartItemCount - eligibleItems.Count());
            }
            catch(Exception ex)
            {
                await orderTransaction.RollbackAsync();
                throw new InvalidOperationException("Unable to Create Order: " , ex);
            }
        }

        private decimal CalculateShipping(ICollection<CartItem> items)
        {
            return items.GroupBy(ci => ci.Variant.Product.VendorId)
                .Sum(g =>Math.Round(g.Sum(ci => ci.Variant.Price * ci.Quantity) * VendorShippingRate,2));
        }

        private decimal CalculateTax(decimal subtotal, decimal discountAmount)
        {
           decimal taxableAmount = subtotal - discountAmount;
           return Math.Round(taxableAmount*TaxRate,2);
        }

        private decimal CalculateTotal(OrderSummary summary)
        {
            decimal taxableAmount = summary.Subtotal - summary.DiscountAmount;
            return taxableAmount + summary.ShippingAmount + summary.TaxAmount;
        }

        private async Task<Order> CreateInitialOrder(int userId, Payment payment, Discount? discount, OrderSummary summary, int userAddressId)
        {
            var order = new Order
            {
                UserId = userId,
                DiscountId = discount?.Id,
                PaymentId = payment.Id,
                Subtotal = summary.Subtotal,
                DiscountAmount = summary.DiscountAmount,
                TaxAmount = summary.TaxAmount,
                ShippingAmount = summary.ShippingAmount,
                PlatformCommission = 20.00m,
                Total = summary.Total,
                Status = OrderStatus.PendingPayment,
                OrderPaymentStatus = PaymentStatus.Pending,
                PlacedAt = DateTime.Now,
                UserAddressId = userAddressId
            };

            return await _orderRepository.Create(order);
        }

        private async Task<Dictionary<int, int>> CreateOrderShipments(int orderId, int userAddressId, IEnumerable<CartItem> eligibleItems)
        {
            var shipmentsMap = new Dictionary<int, int>();
            var vendorGroups = eligibleItems.GroupBy(ci => ci.Variant.Product.VendorId);
            foreach (var vendorGroup in vendorGroups)
            {
                int vendorId = vendorGroup.Key;
                decimal grossAmount = vendorGroup.Sum(ci => ci.Variant.Price * ci.Quantity);
                decimal vendorShipping = Math.Round(grossAmount * VendorShippingRate, 2);

                var shipment = await _shipmentService.ScheduleShipment(orderId, userAddressId, vendorShipping);
                shipmentsMap[vendorId] = shipment.Id;
            }
            return shipmentsMap;
        }

        private async Task CreateOrderItems(int orderId, IEnumerable<CartItem> eligibleItems, Dictionary<int, int> shipmentsMap)
        {
            var orderItems = eligibleItems
                .Select(cartItem => new OrderItem
                {
                    OrderId = orderId,
                    VariantId = cartItem.VariantId,
                    VendorId = cartItem.Variant.Product.VendorId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Variant.Price,
                    ShipmentId = shipmentsMap.GetValueOrDefault(cartItem.Variant.Product.VendorId)
                }).ToList();
            await _orderItemRepository.CreateRange(orderItems);
        }

        private OrderResponse BuildResponse(Order order, int skippedItemCount)
        {
            return new OrderResponse
            {
                OrderId = order.Id,
                Subtotal = order.Subtotal,
                DiscountAmount = order.DiscountAmount,
                TaxAmount = order.TaxAmount,
                ShippingAmount = order.ShippingAmount,
                PlatformCommission = order.PlatformCommission,
                Total = order.Total,
                Currency = "inr",
                Message = skippedItemCount > 0
                    ? $"Order placed successfully. {skippedItemCount} out-of-stock item(s) were skipped. Make payment to initiate shipping."
                    : "Order placed successfully. Make payment to initiate shipping."
            };
        }


        private void ScheduleStockRelease(int orderId)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(StockReleaseDelay);
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        using var transaction = await dbContext.Database.BeginTransactionAsync();

                        var order = await orderRepository.GetOrderWithDetailsById(orderId);
                        if (order != null && order.Status == OrderStatus.PendingPayment)
                        {
                            var variantService = scope.ServiceProvider.GetRequiredService<IProductVariantService>();
                            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                            await variantService.ReleaseStockReservation(orderId);

                            order.Status = OrderStatus.PaymentFailed;
                            order.OrderPaymentStatus = PaymentStatus.Failed;
                            await orderRepository.Update(orderId, order);

                            if (order.Payment != null)
                            {
                                await paymentService.UpdatePaymentToFailed(order.Payment);
                            }

                            await transaction.CommitAsync();
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw new InvalidOperationException("Unable to release stock reservation: ", ex);
                }
            });
        }



        public async Task<OrderSummaryResponse> GetOrderDetails(int orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsById(orderId) ?? throw new KeyNotFoundException($"Order {orderId} not found.");

            var role = _currentUser.Role;
            var userId = _currentUser.UserId;

            if (role == "Customer" && order.UserId != userId)   throw new UnauthorizedAccessException("Access denied.");
            if(role == "Vendor"){
                var vendor = await _vendorRepository.GetByUserId(userId) ?? throw new KeyNotFoundException("Vendor profile not found for current user.");
                if(order.Items.Any(i => i.VendorId != vendor.Id))    throw new UnauthorizedAccessException("Access denied.");
            }

            return _mapper.Map<OrderSummaryResponse>(order);
        }

        public async Task<ICollection<OrderSummaryResponse>> GetMyOrders()
        {
            var orders = await _orderRepository.GetOrdersByUserId(_currentUser.UserId);
            return _mapper.Map<ICollection<OrderSummaryResponse>>(orders);
        }

        public async Task<PageResponse<OrderSummaryResponse>> GetAllOrders(OrderFilterRequest query)
        {
            var (items, totalCount) = await _orderRepository.GetPagedOrders(query.PageNumber, query.PageSize, query.SearchTerm);
            
            return new PageResponse<OrderSummaryResponse>
            {
                Items = _mapper.Map<ICollection<OrderSummaryResponse>>(items),
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ICollection<OrderSummaryResponse>> GetVendorOrders()
        {
            var vendor = await _vendorRepository.GetByUserId(_currentUser.UserId)   ?? throw new KeyNotFoundException("Vendor profile not found for current user.");
            var orders = await _orderRepository.GetOrdersByVendorId(vendor.Id);
            return _mapper.Map<ICollection<OrderSummaryResponse>>(orders);
        }

        private static string GenerateTrackingNumber() => $"TRK{DateTime.Now:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}
