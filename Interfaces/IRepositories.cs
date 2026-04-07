using EventCalendarAPI.Models;

namespace EventCalendarAPI.Interfaces
{
    // ─── Generic Repository ──────────────────────────────────────
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }

    // ─── User ────────────────────────────────────────────────────
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, string? search = null);
    }

    // ─── Event ───────────────────────────────────────────────────
    public interface IEventRepository : IRepository<Event>
    {
        Task<Event?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Event>> GetAllWithDetailsAsync();
        Task<IEnumerable<Event>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Event>> SearchAsync(string? keyword, int? categoryId, int? venueId, DateTime? startDate, DateTime? endDate, EventPrivacy? privacy, decimal? minPrice, decimal? maxPrice, int page, int pageSize);
        Task<int> GetSearchCountAsync(string? keyword, int? categoryId, int? venueId, DateTime? startDate, DateTime? endDate, EventPrivacy? privacy, decimal? minPrice, decimal? maxPrice);
        Task<PagedResult<Event>> GetAllPagedAsync(int page, int pageSize);
    }

    // ─── Category ────────────────────────────────────────────────
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<bool> NameExistsAsync(string name);
        Task<PagedResult<Category>> GetPagedAsync(int page, int pageSize);
    }

    // ─── Venue ───────────────────────────────────────────────────
    public interface IVenueRepository : IRepository<Venue>
    {
        Task<IEnumerable<Venue>> GetActiveVenuesAsync();
        Task<PagedResult<Venue>> GetPagedAsync(int page, int pageSize, string? city = null, string? country = null);
    }

    // ─── Ticket ──────────────────────────────────────────────────
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<Ticket?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Ticket>> GetByEventIdAsync(int eventId);
        Task<IEnumerable<Ticket>> GetByUserIdAsync(int userId);
        Task<Ticket?> GetByTicketNumberAsync(string ticketNumber);
        Task<PagedResult<Ticket>> GetPagedAsync(int page, int pageSize, string? status = null);
    }

    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetByTicketIdAsync(int ticketId);
        Task<PagedResult<Payment>> GetPagedAsync(int page, int pageSize, string? status = null);
    }

    // ─── Reminder ────────────────────────────────────────────────
    public interface IReminderRepository : IRepository<Reminder>
    {
        Task<IEnumerable<Reminder>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Reminder>> GetByEventIdAsync(int eventId);
        Task<IEnumerable<Reminder>> GetPendingRemindersAsync(DateTime upTo);
        Task<PagedResult<Reminder>> GetPagedByUserAsync(int userId, int page, int pageSize);
    }

    // ─── AuditLog ────────────────────────────────────────────────
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log);
        Task<PagedResult<AuditLog>> GetPagedAsync(int page, int pageSize, string? action, string? entityType, DateTime? from, DateTime? to);
    }

    // ─── Ticket (filtered) ───────────────────────────────────────
    public interface ITicketFilterRepository
    {
        Task<PagedResult<Ticket>> GetPagedFilteredAsync(int page, int pageSize, string? status, int? eventId);
    }

    // ─── Payment (filtered) ──────────────────────────────────────
    public interface IPaymentFilterRepository
    {
        Task<PagedResult<Payment>> GetPagedFilteredAsync(int page, int pageSize, string? status, string? method);
    }

    // ─── Notification ────────────────────────────────────────────
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
        Task MarkAllReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
    }

    // ─── Paged Result ────────────────────────────────────────────
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
