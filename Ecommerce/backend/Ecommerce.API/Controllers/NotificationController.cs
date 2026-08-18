using Ecommerce.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public NotificationController(INotificationService notificationService, ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var notifications = await _notificationService.GetUserNotifications(_currentUserService.UserId);
            return Ok(notifications);
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var result = await _notificationService.MarkAllNotificationsAsRead(_currentUserService.UserId);
            return Ok(new { success = result });
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkNotificationAsRead(id);
            return Ok(new { success = result });
        }
    }
}
