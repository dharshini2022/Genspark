using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.DAL.Context;

namespace Ecommerce.BLL
{
    public class JobExecutor : IJobExecutor
    {
        private readonly IShipmentService _shipmentService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductVariantService _productVariantService;
        private readonly IDiscountService _discountService;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly AppDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly IVendorRepository _vendorRepository;

        public JobExecutor(
            IShipmentService shipmentService,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductVariantService productVariantService,
            IDiscountService discountService,
            IPaymentService paymentService,
            INotificationService notificationService,
            AppDbContext dbContext,
            IEmailService emailService,
            IVendorRepository vendorRepository)
        {
            _shipmentService = shipmentService;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productVariantService = productVariantService;
            _discountService = discountService;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _dbContext = dbContext;
            _emailService = emailService;
            _vendorRepository = vendorRepository;
        }

        public async Task DeliverOrder(int orderId)
        {
            var orderItems = await _orderItemRepository.GetOrderItemsByOrderId(orderId);
            var shipmentIds = orderItems
                .Where(oi => oi.ShipmentId.HasValue)
                .Select(oi => oi.ShipmentId!.Value)
                .Distinct()
                .ToList();

            foreach (var shipmentId in shipmentIds)
            {
                await _shipmentService.UpdateShipmentStatus(shipmentId, ShipmentStatus.Delivered);
            }

            var order = await _orderRepository.GetById(orderId);
            if (order != null)
            {
                order.Status = OrderStatus.Delivered;
                await _orderRepository.Update(orderId, order);
            }
        }

        public async Task ReleaseStock(int orderId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsById(orderId);
                if (order != null && order.Status == OrderStatus.PendingPayment)
                {
                    await _productVariantService.ReleaseStockReservation(orderId);
                    await _discountService.ReleaseDiscountReservation(orderId);

                    order.Status = OrderStatus.PaymentFailed;
                    order.OrderPaymentStatus = PaymentStatus.Failed;
                    await _orderRepository.Update(orderId, order);

                    if (order.Payment != null)
                    {
                        await _paymentService.UpdatePaymentToFailed(order.Payment);
                    }

                    await transaction.CommitAsync();
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ProcessWishlistReminders()
        {
            var cutoff = DateTime.Now.AddDays(-1);

            var itemsToRemind = await _dbContext.WishlistItems
                .Include(wi => wi.Wishlist)
                    .ThenInclude(w => w.User)
                .Include(wi => wi.Variant)
                    .ThenInclude(v => v.Product)
                .Where(wi => wi.AddedAt <= cutoff)
                .ToListAsync();

            var itemsGroupedByUser = itemsToRemind
                .GroupBy(wi => wi.Wishlist.UserId)
                .ToList();

            foreach (var group in itemsGroupedByUser)
            {
                var userId = group.Key;
                var unremindedItems = new List<WishlistItem>();

                foreach (var item in group)
                {
                    bool alreadyNotified = await _dbContext.Notifications.AnyAsync(n =>
                        n.UserId == userId &&
                        n.Type == NotificationType.WishlistReminder &&
                        n.CreatedAt >= item.AddedAt);

                    if (!alreadyNotified)
                    {
                        unremindedItems.Add(item);
                    }
                }

                if (unremindedItems.Any())
                {
                    var productNames = unremindedItems
                        .Select(wi => wi.Variant.Product?.Name ?? "Product")
                        .Distinct()
                        .ToList();

                    string title = "Items waiting in your wishlist!";
                    string message = productNames.Count == 1
                        ? $"You left '{productNames[0]}' in your wishlist. Don't forget to check out!"
                        : $"You have {productNames.Count} items (including '{productNames[0]}') waiting in your wishlist. Don't forget to check out!";

                    await _notificationService.CreateNotification(
                        userId,
                        NotificationType.WishlistReminder,
                        NotificationLevel.Info,
                        title,
                        message
                    );
                }
            }
        }
        public async Task CleanUpExpiredRefreshTokens()
        {
            var cutoff = DateTime.Now;
            await _dbContext.RefreshTokens
                .Where(rt => rt.ExpiresAt < cutoff)
                .ExecuteDeleteAsync();
        }

        public async Task SendOrderConfirmationNotifications(int orderId)
        {
            var bgOrder = await _dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (bgOrder != null)
            {
                try
                {
                    await _notificationService.CreateNotification(
                        bgOrder.UserId,
                        NotificationType.OrderPlaced,
                        NotificationLevel.Success,
                        "Order Confirmed",
                        $"Your order #{bgOrder.Id} of amount ₹{bgOrder.Total:F2} has been successfully paid and confirmed!"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Notification Error] Failed to create order notification: {ex.Message}");
                }

                try
                {
                    var vendorIds = bgOrder.Items
                        .Select(i => i.VendorId)
                        .Distinct()
                        .ToList();

                    foreach (var vendorId in vendorIds)
                    {
                        var vendor = await _vendorRepository.GetById(vendorId);
                        if (vendor != null)
                        {
                            await _notificationService.CreateNotification(
                                vendor.UserId,
                                NotificationType.OrderPlaced,
                                NotificationLevel.Success,
                                "New Order Received",
                                $"New order #{bgOrder.Id} received! Items from your store have been ordered."
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Notification Error] Failed to notify vendors: {ex.Message}");
                }

                try
                {
                    await _emailService.SendOrderConfirmationEmail(bgOrder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Email Error] Failed to send order confirmation email: {ex.Message}");
                }
            }
        }
    }
}
