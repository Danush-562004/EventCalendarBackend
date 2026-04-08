using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;

namespace EventCalendarAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetMyNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            return notifications.Select(MapToResponse);
        }

        public async Task<int> GetUnreadCountAsync(int userId) =>
            await _notificationRepository.GetUnreadCountAsync(userId);

        public async Task MarkReadAsync(int id, int userId) =>
            await _notificationRepository.MarkReadAsync(id, userId);

        public async Task MarkAllReadAsync(int userId) =>
            await _notificationRepository.MarkAllReadAsync(userId);

        private static NotificationResponseDto MapToResponse(Notification n) => new()
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type.ToString(),
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            UserId = n.UserId
        };
    }
}
