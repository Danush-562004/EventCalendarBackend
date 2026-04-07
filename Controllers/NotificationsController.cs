using EventCalendarAPI.Controllers;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendarAPI.Controllers
{
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>Get all notifications for the current user.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<NotificationResponseDto>>), 200)]
        public async Task<IActionResult> GetMine()
        {
            var notifications = await _notificationService.GetMyNotificationsAsync(GetCurrentUserId());
            return Ok(ApiResponseDto<IEnumerable<NotificationResponseDto>>.Ok(notifications));
        }

        /// <summary>Get unread notification count for the current user.</summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponseDto<int>), 200)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync(GetCurrentUserId());
            return Ok(ApiResponseDto<int>.Ok(count));
        }

        /// <summary>Mark all notifications as read.</summary>
        [HttpPost("mark-all-read")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllReadAsync(GetCurrentUserId());
            return Ok(ApiResponseDto<object>.Ok(new { }, "All notifications marked as read."));
        }
    }
}
