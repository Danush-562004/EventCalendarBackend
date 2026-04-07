using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Models;

namespace EventCalendarAPI.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    }

    public interface IUserService
    {
        Task<UserResponseDto> GetByIdAsync(int id);
        Task<PagedResponseDto<UserResponseDto>> GetAllAsync(int page, int pageSize, string? search = null);
        Task<UserResponseDto> UpdateAsync(int id, UpdateUserRequestDto request, int requestingUserId);
        Task DeleteAsync(int id, int requestingUserId);
        Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
    }

    public interface IEventService
    {
        Task<EventResponseDto> GetByIdAsync(int id);
        Task<PagedResponseDto<EventResponseDto>> GetAllAsync(int page, int pageSize);
        Task<PagedResponseDto<EventResponseDto>> SearchAsync(EventFilterRequestDto filter);
        Task<IEnumerable<EventResponseDto>> GetByCurrentUserAsync(int userId);
        Task<EventResponseDto> CreateAsync(CreateEventRequestDto request, int userId);
        Task<EventResponseDto> UpdateAsync(int id, UpdateEventRequestDto request, int userId);
        Task DeleteAsync(int id, int userId);
    }

    public interface ICategoryService
    {
        Task<CategoryResponseDto> GetByIdAsync(int id);
        Task<PagedResponseDto<CategoryResponseDto>> GetAllAsync(int page, int pageSize);
        Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request);
        Task<CategoryResponseDto> UpdateAsync(int id, UpdateCategoryRequestDto request);
        Task DeleteAsync(int id);
    }

    public interface IVenueService
    {
        Task<VenueResponseDto> GetByIdAsync(int id);
        Task<PagedResponseDto<VenueResponseDto>> GetAllAsync(int page, int pageSize, string? city = null, string? country = null);
        Task<VenueResponseDto> CreateAsync(CreateVenueRequestDto request);
        Task<VenueResponseDto> UpdateAsync(int id, UpdateVenueRequestDto request);
        Task DeleteAsync(int id);
    }

    public interface ITicketService
    {
        Task<TicketResponseDto> GetByIdAsync(int id, int requestingUserId, string userRole);
        Task<PagedResponseDto<TicketResponseDto>> GetAllAsync(int page, int pageSize, string? status = null);
        Task<IEnumerable<TicketResponseDto>> GetByEventIdAsync(int eventId, int requestingUserId, string userRole);
        Task<IEnumerable<TicketResponseDto>> GetByCurrentUserAsync(int userId);
        Task<TicketResponseDto> CreateAsync(CreateTicketRequestDto request, int userId);
        Task<TicketResponseDto> UpdateAsync(int id, UpdateTicketRequestDto request, int requestingUserId, string userRole);
        Task DeleteAsync(int id, int requestingUserId, string userRole);
    }

    public interface IPaymentService
    {
        Task<PaymentResponseDto> GetByIdAsync(int id);
        Task<PagedResponseDto<PaymentResponseDto>> GetAllAsync(int page, int pageSize, string? status = null);
        Task<IEnumerable<PaymentResponseDto>> GetByTicketIdAsync(int ticketId);
        Task<PaymentResponseDto> CreateAsync(CreatePaymentRequestDto request);
        Task<PaymentResponseDto> UpdateAsync(int id, UpdatePaymentRequestDto request);
        Task DeleteAsync(int id);
    }

    public interface IReminderService
    {
        Task<ReminderResponseDto> GetByIdAsync(int id, int requestingUserId);
        Task<PagedResponseDto<ReminderResponseDto>> GetByCurrentUserAsync(int userId, int page, int pageSize);
        Task<IEnumerable<ReminderResponseDto>> GetByEventIdAsync(int eventId, int requestingUserId);
        Task<ReminderResponseDto> CreateAsync(CreateReminderRequestDto request, int userId);
        Task<ReminderResponseDto> UpdateAsync(int id, UpdateReminderRequestDto request, int requestingUserId);
        Task DeleteAsync(int id, int requestingUserId);
    }

    public interface IPasswordService
    {
        string HashPassword(string password, out byte[] salt);
        bool VerifyPassword(string password, string hash, byte[] salt);
    }

    public interface ITokenService
    {
        string GenerateToken(int userId, string username, string role);
        DateTime GetTokenExpiry();
    }

    public interface IAuditLogService
    {
        Task LogAsync(string action, string entityType, string? entityId = null,
            int? userId = null, string? userName = null,
            string? oldValues = null, string? newValues = null, string? ipAddress = null);
        Task<PagedResponseDto<AuditLogResponseDto>> GetAllAsync(int page, int pageSize, string? action, string? entityType, DateTime? from, DateTime? to);
    }

    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponseDto>> GetMyNotificationsAsync(int userId);
        Task MarkAllReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
    }

    public interface IRefundService
    {
        Task ProcessRefundsForEventAsync(int eventId, string eventTitle, DateTime eventStartDateTime);
    }
}
