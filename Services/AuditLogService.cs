using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;

namespace EventCalendarAPI.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repo;

        public AuditLogService(IAuditLogRepository repo)
        {
            _repo = repo;
        }

        public async Task LogAsync(string action, string entityType, string? entityId = null,
            int? userId = null, string? userName = null,
            string? oldValues = null, string? newValues = null, string? ipAddress = null)
        {
            var log = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                UserName = userName ?? "System",
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };
            await _repo.AddAsync(log);
        }

        public async Task<PagedResponseDto<AuditLogResponseDto>> GetAllAsync(int page, int pageSize,
            string? action, string? entityType, DateTime? from, DateTime? to)
        {
            var result = await _repo.GetPagedAsync(page, pageSize, action, entityType, from, to);
            return new PagedResponseDto<AuditLogResponseDto>
            {
                Items = result.Items.Select(MapToResponse).ToList(),
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private static AuditLogResponseDto MapToResponse(AuditLog a) => new()
        {
            Id = a.Id,
            UserId = a.UserId,
            UserName = a.UserName,
            Action = a.Action,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            IpAddress = a.IpAddress,
            Timestamp = a.Timestamp
        };
    }
}
