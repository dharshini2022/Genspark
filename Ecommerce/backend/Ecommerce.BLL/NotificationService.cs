using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Microsoft.AspNetCore.SignalR;
using Ecommerce.BLL.Hubs;

namespace Ecommerce.BLL
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        public async Task<Notification> CreateNotification(int userId, NotificationType type, NotificationLevel level, string title, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Level = level,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            var created = await _notificationRepository.Create(notification);

            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", created);

            return created;
        }

        public async Task<ICollection<Notification>> GetUserNotifications(int userId)
        {
            return await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        }

        public async Task<bool> MarkNotificationAsRead(int notificationId)
        {
            var notification = await _notificationRepository.GetById(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            await _notificationRepository.Update(notificationId, notification);
            return true;
        }

        public async Task<bool> MarkAllNotificationsAsRead(int userId)
        {
            return await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public Task<bool> SendEmailVerificationOtp(string email, string otp)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SendEmailConfirmation(string email, string title, string message)
        {
            return Task.FromResult(true);
        }
    }
}
