using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendarAPI.Controllers
{
    [Authorize]
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
            var result = await _notificationService.GetMyNotificationsAsync(GetCurrentUserId());
            return Ok(ApiResponseDto<IEnumerable<NotificationResponseDto>>.Ok(result));
        }

        /// <summary>Get unread notification count.</summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponseDto<int>), 200)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync(GetCurrentUserId());
            return Ok(ApiResponseDto<int>.Ok(count));
        }

        /// <summary>Mark a single notification as read.</summary>
        [HttpPut("{id:int}/read")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        public async Task<IActionResult> MarkRead([FromRoute] int id)
        {
            await _notificationService.MarkReadAsync(id, GetCurrentUserId());
            return Ok(ApiResponseDto<object>.Ok(null!, "Notification marked as read."));
        }

        /// <summary>Mark all notifications as read.</summary>
        [HttpPut("read-all")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllReadAsync(GetCurrentUserId());
            return Ok(ApiResponseDto<object>.Ok(null!, "All notifications marked as read."));
        }
    }
}
