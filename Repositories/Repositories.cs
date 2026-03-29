using EventCalendarAPI.Data;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCalendarAPI.Repositories
{
    // ─── User Repository ─────────────────────────────────────────
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User?> GetByUsernameAsync(string username) =>
            await _dbSet.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        public async Task<User?> GetByEmailAsync(string email) =>
            await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail) =>
            await _dbSet.FirstOrDefaultAsync(u =>
                u.Username.ToLower() == usernameOrEmail.ToLower() ||
                u.Email.ToLower() == usernameOrEmail.ToLower());

        public async Task<bool> UsernameExistsAsync(string username) =>
            await _dbSet.AnyAsync(u => u.Username.ToLower() == username.ToLower());

        public async Task<bool> EmailExistsAsync(string email) =>
            await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());

        public async Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(s) ||
                    u.FirstName.ToLower().Contains(s) ||
                    u.LastName.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s));
            }
            query = query.OrderBy(u => u.Username);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<User> { Items = items, TotalCount = total };
        }
    }

    // ─── Event Repository ────────────────────────────────────────
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Event?> GetByIdWithDetailsAsync(int id) =>
            await _dbSet
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

        public async Task<IEnumerable<Event>> GetAllWithDetailsAsync() =>
            await _dbSet
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Tickets)
                .Where(e => e.IsActive)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

        public async Task<PagedResult<Event>> GetAllPagedAsync(int page, int pageSize)
        {
            var query = _dbSet
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Tickets)
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.StartDateTime);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<Event> { Items = items, TotalCount = total };
        }

        public async Task<IEnumerable<Event>> GetByUserIdAsync(int userId) =>
            await _dbSet
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Tickets)
                .Where(e => e.UserId == userId && e.IsActive)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

        public async Task<IEnumerable<Event>> SearchAsync(string? keyword, int? categoryId, DateTime? startDate, DateTime? endDate, EventPrivacy? privacy, decimal? minPrice, decimal? maxPrice, int page, int pageSize, int? venueId = null)
        {
            var query = BuildSearchQuery(keyword, categoryId, venueId, startDate, endDate, privacy, minPrice, maxPrice);
            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<int> GetSearchCountAsync(string? keyword, int? categoryId, DateTime? startDate, DateTime? endDate, EventPrivacy? privacy, decimal? minPrice, decimal? maxPrice, int? venueId = null)
        {
            var query = BuildSearchQuery(keyword, categoryId, venueId, startDate, endDate, privacy, minPrice, maxPrice);
            return await query.CountAsync();
        }

        private IQueryable<Event> BuildSearchQuery(string? keyword, int? categoryId, int? venueId, DateTime? startDate, DateTime? endDate, EventPrivacy? privacy, decimal? minPrice, decimal? maxPrice)
        {
            var query = _dbSet
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Tickets)
                .Where(e => e.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(e =>
                    e.Title.Contains(keyword) ||
                    (e.Description != null && e.Description.Contains(keyword)) ||
                    (e.Location != null && e.Location.Contains(keyword)));

            if (categoryId.HasValue)
                query = query.Where(e => e.CategoryId == categoryId.Value);

            if (venueId.HasValue)
                query = query.Where(e => e.VenueId == venueId.Value);

            if (startDate.HasValue)
                query = query.Where(e => e.StartDateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.StartDateTime < endDate.Value);

            if (privacy.HasValue)
                query = query.Where(e => e.Privacy == privacy.Value);

            if (minPrice.HasValue)
                query = query.Where(e => e.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(e => e.Price <= maxPrice.Value);

            return query.OrderByDescending(e => e.StartDateTime);
        }
    }

    // ─── Category Repository ─────────────────────────────────────
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> NameExistsAsync(string name) =>
            await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower());

        public async Task<PagedResult<Category>> GetPagedAsync(int page, int pageSize)
        {
            var query = _dbSet.Where(c => c.IsActive).OrderBy(c => c.Name);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<Category> { Items = items, TotalCount = total };
        }
    }

    // ─── Venue Repository ────────────────────────────────────────
    public class VenueRepository : Repository<Venue>, IVenueRepository
    {
        public VenueRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Venue>> GetActiveVenuesAsync() =>
            await _dbSet.Where(v => v.IsActive).OrderBy(v => v.Name).ToListAsync();

        public async Task<PagedResult<Venue>> GetPagedAsync(int page, int pageSize, string? city = null, string? country = null)
        {
            var query = _dbSet.Where(v => v.IsActive).AsQueryable();
            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(v => v.City.ToLower().Contains(city.ToLower()));
            if (!string.IsNullOrWhiteSpace(country))
                query = query.Where(v => v.Country.ToLower().Contains(country.ToLower()));
            query = query.OrderBy(v => v.Name);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<Venue> { Items = items, TotalCount = total };
        }
    }

    // ─── Ticket Repository ───────────────────────────────────────
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Ticket?> GetByIdWithDetailsAsync(int id) =>
            await _dbSet
                .Include(t => t.Event)
                .Include(t => t.User)
                .Include(t => t.Payments)
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<Ticket>> GetByEventIdAsync(int eventId) =>
            await _dbSet
                .Include(t => t.User)
                .Include(t => t.Payments)
                .Where(t => t.EventId == eventId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Ticket>> GetByUserIdAsync(int userId) =>
            await _dbSet
                .Include(t => t.Event)
                .Include(t => t.Payments)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<Ticket?> GetByTicketNumberAsync(string ticketNumber) =>
            await _dbSet.FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);

        public async Task<PagedResult<Ticket>> GetPagedAsync(int page, int pageSize, string? status = null)
        {
            var query = _dbSet
                .Include(t => t.Event)
                .Include(t => t.User)
                .Include(t => t.Payments)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, true, out var ts))
                query = query.Where(t => t.Status == ts);
            query = query.OrderByDescending(t => t.CreatedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<Ticket> { Items = items, TotalCount = total };
        }
    }

    // ─── Payment Repository ──────────────────────────────────────
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetByTicketIdAsync(int ticketId) =>
            await _dbSet
                .Where(p => p.TicketId == ticketId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<PagedResult<Payment>> GetPagedAsync(int page, int pageSize, string? status = null)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var ps))
                query = query.Where(p => p.Status == ps);
            query = query.OrderByDescending(p => p.CreatedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<Payment> { Items = items, TotalCount = total };
        }
    }

    // ─── Reminder Repository ─────────────────────────────────────
    public class ReminderRepository : Repository<Reminder>, IReminderRepository
    {
        public ReminderRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Reminder>> GetByUserIdAsync(int userId) =>
            await _dbSet
                .Include(r => r.Event)
                .Where(r => r.UserId == userId && r.IsActive)
                .OrderBy(r => r.ReminderDateTime)
                .ToListAsync();

        public async Task<IEnumerable<Reminder>> GetByEventIdAsync(int eventId) =>
            await _dbSet
                .Where(r => r.EventId == eventId && r.IsActive)
                .OrderBy(r => r.ReminderDateTime)
                .ToListAsync();

        public async Task<IEnumerable<Reminder>> GetPendingRemindersAsync(DateTime upTo) =>
            await _dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.IsActive && !r.IsSent && r.ReminderDateTime <= upTo)
                .ToListAsync();

        public async Task<PagedResult<Reminder>> GetPagedByUserAsync(int userId, int page, int pageSize)
        {
            var query = _dbSet
                .Include(r => r.Event)
                .Where(r => r.UserId == userId && r.IsActive)
                .OrderBy(r => r.ReminderDateTime);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<Reminder> { Items = items, TotalCount = total };
        }
    }

    // ─── AuditLog Repository ─────────────────────────────────────
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;
        public AuditLogRepository(ApplicationDbContext context) { _context = context; }

        public async Task AddAsync(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<AuditLog>> GetPagedAsync(int page, int pageSize, string? action, string? entityType, DateTime? from, DateTime? to)
        {
            var query = _context.AuditLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.Action.ToLower() == action.ToLower());
            if (!string.IsNullOrWhiteSpace(entityType))
                query = query.Where(a => a.EntityType.ToLower() == entityType.ToLower());
            if (from.HasValue)
                query = query.Where(a => a.Timestamp >= from.Value);
            if (to.HasValue)
                query = query.Where(a => a.Timestamp <= to.Value);
            query = query.OrderByDescending(a => a.Timestamp);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<AuditLog> { Items = items, TotalCount = total };
        }
    }
}
