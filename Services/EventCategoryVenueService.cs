using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;

namespace EventCalendarAPI.Services
{
    // ─── Event Service ───────────────────────────────────────────
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly IAuditLogService _auditLog;
        private readonly IRefundService _refundService;

        public EventService(IEventRepository eventRepository, ICategoryRepository categoryRepository,
            IVenueRepository venueRepository, IAuditLogService auditLog, IRefundService refundService)
        {
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
            _venueRepository = venueRepository;
            _auditLog = auditLog;
            _refundService = refundService;
        }

        public async Task<EventResponseDto> GetByIdAsync(int id)
        {
            var ev = await _eventRepository.GetByIdWithDetailsAsync(id)
                ?? throw new EntityNotFoundException("Event", id);
            return MapToResponse(ev);
        }

        public async Task<PagedResponseDto<EventResponseDto>> GetAllAsync(int page, int pageSize)
        {
            var result = await _eventRepository.GetAllPagedAsync(page, pageSize);
            return new PagedResponseDto<EventResponseDto>
            {
                Items = result.Items.Select(MapToResponse).ToList(),
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResponseDto<EventResponseDto>> SearchAsync(EventFilterRequestDto filter)
        {
            var items = await _eventRepository.SearchAsync(filter.Keyword, filter.CategoryId, filter.VenueId,
                filter.StartDate, filter.EndDate, filter.Privacy, filter.MinPrice, filter.MaxPrice,
                filter.Page, filter.PageSize);
            var total = await _eventRepository.GetSearchCountAsync(filter.Keyword, filter.CategoryId, filter.VenueId,
                filter.StartDate, filter.EndDate, filter.Privacy, filter.MinPrice, filter.MaxPrice);

            return new PagedResponseDto<EventResponseDto>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<IEnumerable<EventResponseDto>> GetByCurrentUserAsync(int userId)
        {
            var events = await _eventRepository.GetByUserIdAsync(userId);
            return events.Select(MapToResponse);
        }

        public async Task<EventResponseDto> CreateAsync(CreateEventRequestDto request, int userId)
        {
            if (request.StartDateTime >= request.EndDateTime)
                throw new ValidationException("Start date/time must be before end date/time.");

            if (!await _categoryRepository.ExistsAsync(request.CategoryId))
                throw new EntityNotFoundException("Category", request.CategoryId);

            // Venue capacity check
            if (request.VenueId.HasValue && request.MaxAttendees > 0)
            {
                var venue = await _venueRepository.GetByIdAsync(request.VenueId.Value);
                if (venue != null && request.MaxAttendees > venue.Capacity)
                    throw new ValidationException($"Max attendees ({request.MaxAttendees}) cannot exceed venue capacity ({venue.Capacity}).");
            }

            var ev = new Event
            {
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                Location = request.Location,
                ReminderEnabled = request.ReminderEnabled,
                ReminderMinutesBefore = request.ReminderMinutesBefore,
                MaxAttendees = request.MaxAttendees,
                CategoryId = request.CategoryId,
                VenueId = request.VenueId,
                UserId = userId
            };

            await _eventRepository.AddAsync(ev);
            var created = await _eventRepository.GetByIdWithDetailsAsync(ev.Id)
                ?? throw new Exception("Failed to retrieve created event.");
            await _auditLog.LogAsync("Create", "Event", created.Id.ToString(), userId, null,
                newValues: $"{{\"title\":\"{created.Title}\"}}");
            return MapToResponse(created);
        }

        public async Task<EventResponseDto> UpdateAsync(int id, UpdateEventRequestDto request, int userId)
        {
            var ev = await _eventRepository.GetByIdWithDetailsAsync(id)
                ?? throw new EntityNotFoundException("Event", id);

            // Admin owns all events — no ownership check needed
            var old = $"{{\"title\":\"{ev.Title}\"}}";

            if (request.Title != null) ev.Title = request.Title;
            if (request.Description != null) ev.Description = request.Description;
            if (request.Price.HasValue) ev.Price = request.Price.Value;
            if (request.StartDateTime.HasValue) ev.StartDateTime = request.StartDateTime.Value;
            if (request.EndDateTime.HasValue) ev.EndDateTime = request.EndDateTime.Value;
            if (request.Location != null) ev.Location = request.Location;
            if (request.ReminderEnabled.HasValue) ev.ReminderEnabled = request.ReminderEnabled.Value;
            if (request.ReminderMinutesBefore.HasValue) ev.ReminderMinutesBefore = request.ReminderMinutesBefore;
            if (request.MaxAttendees.HasValue) ev.MaxAttendees = request.MaxAttendees.Value;
            if (request.CategoryId.HasValue) ev.CategoryId = request.CategoryId.Value;
            if (request.VenueId.HasValue) ev.VenueId = request.VenueId;
            ev.UpdatedAt = DateTime.UtcNow;

            if (ev.StartDateTime >= ev.EndDateTime)
                throw new ValidationException("Start date/time must be before end date/time.");

            // Venue capacity check
            if (ev.VenueId.HasValue && ev.MaxAttendees > 0)
            {
                var venue = await _venueRepository.GetByIdAsync(ev.VenueId.Value);
                if (venue != null && ev.MaxAttendees > venue.Capacity)
                    throw new ValidationException($"Max attendees ({ev.MaxAttendees}) cannot exceed venue capacity ({venue.Capacity}).");
            }

            await _eventRepository.UpdateAsync(ev);
            await _auditLog.LogAsync("Update", "Event", id.ToString(), userId, null, old,
                $"{{\"title\":\"{ev.Title}\"}}");
            var updated = await _eventRepository.GetByIdWithDetailsAsync(ev.Id)!;
            return MapToResponse(updated!);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var ev = await _eventRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Event", id);

            // Admin owns all events — no ownership check needed
            ev.IsActive = false;
            ev.UpdatedAt = DateTime.UtcNow;
            await _eventRepository.UpdateAsync(ev);
            await _auditLog.LogAsync("Delete", "Event", id.ToString(), userId, null,
                oldValues: $"{{\"title\":\"{ev.Title}\"}}");

            // Issue refunds for all users with completed payments (100% if >2 days before start, 50% if ≤2 days)
            await _refundService.ProcessRefundsForEventAsync(ev.Id, ev.Title, ev.StartDateTime);
        }

        public static EventResponseDto MapToResponse(Event e) => new()
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Price = e.Price,
            AvailableSeats = e.MaxAttendees - (e.Tickets?.Where(t => t.Status != TicketStatus.Cancelled)
                                             .Sum(t => t.Quantity) ?? 0),
            StartDateTime = e.StartDateTime,
            EndDateTime = e.EndDateTime,
            Location = e.Location,
            ReminderEnabled = e.ReminderEnabled,
            ReminderMinutesBefore = e.ReminderMinutesBefore,
            MaxAttendees = e.MaxAttendees,
            TicketCount = e.Tickets?.Count ?? 0,
            IsActive = e.IsActive,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            UserId = e.UserId,
            OrganizerName = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : string.Empty,
            Category = e.Category != null ? new CategoryResponseDto
            {
                Id = e.Category.Id,
                Name = e.Category.Name,
                Description = e.Category.Description,
                ColorCode = e.Category.ColorCode,
                IsActive = e.Category.IsActive,
                CreatedAt = e.Category.CreatedAt
            } : null!,
            Venue = e.Venue != null ? new VenueResponseDto
            {
                Id = e.Venue.Id,
                Name = e.Venue.Name,
                Address = e.Venue.Address,
                City = e.Venue.City,
                State = e.Venue.State,
                Country = e.Venue.Country,
                ZipCode = e.Venue.ZipCode,
                Capacity = e.Venue.Capacity,
                ContactEmail = e.Venue.ContactEmail,
                ContactPhone = e.Venue.ContactPhone,
                IsActive = e.Venue.IsActive,
                CreatedAt = e.Venue.CreatedAt
            } : null
        };
    }

    // ─── Category Service ────────────────────────────────────────
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuditLogService _auditLog;

        public CategoryService(ICategoryRepository categoryRepository, IAuditLogService auditLog)
        {
            _categoryRepository = categoryRepository;
            _auditLog = auditLog;
        }

        public async Task<CategoryResponseDto> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Category", id);
            return MapToResponse(category);
        }

        public async Task<PagedResponseDto<CategoryResponseDto>> GetAllAsync(int page, int pageSize)
        {
            var result = await _categoryRepository.GetPagedAsync(page, pageSize);
            return new PagedResponseDto<CategoryResponseDto>
            {
                Items = result.Items.Select(MapToResponse).ToList(),
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request)
        {
            if (await _categoryRepository.NameExistsAsync(request.Name))
                throw new DuplicateEntityException($"Category '{request.Name}' already exists.");

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                ColorCode = request.ColorCode
            };

            await _categoryRepository.AddAsync(category);
            await _auditLog.LogAsync("Create", "Category", category.Id.ToString(), newValues: $"{{\"name\":\"{category.Name}\"}}");
            return MapToResponse(category);
        }

        public async Task<CategoryResponseDto> UpdateAsync(int id, UpdateCategoryRequestDto request)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Category", id);

            var old = $"{{\"name\":\"{category.Name}\"}}";
            if (request.Name != null) category.Name = request.Name;
            if (request.Description != null) category.Description = request.Description;
            if (request.ColorCode != null) category.ColorCode = request.ColorCode;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);
            await _auditLog.LogAsync("Update", "Category", id.ToString(), oldValues: old, newValues: $"{{\"name\":\"{category.Name}\"}}");
            return MapToResponse(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Category", id);

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.UpdateAsync(category);
            await _auditLog.LogAsync("Delete", "Category", id.ToString(), oldValues: $"{{\"name\":\"{category.Name}\"}}");
        }

        public static CategoryResponseDto MapToResponse(Category c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ColorCode = c.ColorCode,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        };
    }

    // ─── Venue Service ───────────────────────────────────────────
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IAuditLogService _auditLog;

        public VenueService(IVenueRepository venueRepository, IAuditLogService auditLog)
        {
            _venueRepository = venueRepository;
            _auditLog = auditLog;
        }

        public async Task<VenueResponseDto> GetByIdAsync(int id)
        {
            var venue = await _venueRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Venue", id);
            return MapToResponse(venue);
        }

        public async Task<PagedResponseDto<VenueResponseDto>> GetAllAsync(int page, int pageSize, string? city = null, string? country = null)
        {
            var result = await _venueRepository.GetPagedAsync(page, pageSize, city, country);
            return new PagedResponseDto<VenueResponseDto>
            {
                Items = result.Items.Select(MapToResponse).ToList(),
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<VenueResponseDto> CreateAsync(CreateVenueRequestDto request)
        {
            var venue = new Venue
            {
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Country = request.Country,
                ZipCode = request.ZipCode,
                Capacity = request.Capacity,
                Description = request.Description,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone
            };

            await _venueRepository.AddAsync(venue);
            await _auditLog.LogAsync("Create", "Venue", venue.Id.ToString(), newValues: $"{{\"name\":\"{venue.Name}\"}}");
            return MapToResponse(venue);
        }

        public async Task<VenueResponseDto> UpdateAsync(int id, UpdateVenueRequestDto request)
        {
            var venue = await _venueRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Venue", id);

            var old = $"{{\"name\":\"{venue.Name}\"}}";
            if (request.Name != null) venue.Name = request.Name;
            if (request.Address != null) venue.Address = request.Address;
            if (request.City != null) venue.City = request.City;
            if (request.State != null) venue.State = request.State;
            if (request.Country != null) venue.Country = request.Country;
            if (request.ZipCode != null) venue.ZipCode = request.ZipCode;
            if (request.Capacity.HasValue) venue.Capacity = request.Capacity.Value;
            if (request.Description != null) venue.Description = request.Description;
            if (request.ContactEmail != null) venue.ContactEmail = request.ContactEmail;
            if (request.ContactPhone != null) venue.ContactPhone = request.ContactPhone;
            venue.UpdatedAt = DateTime.UtcNow;

            await _venueRepository.UpdateAsync(venue);
            await _auditLog.LogAsync("Update", "Venue", id.ToString(), oldValues: old, newValues: $"{{\"name\":\"{venue.Name}\"}}");
            return MapToResponse(venue);
        }

        public async Task DeleteAsync(int id)
        {
            var venue = await _venueRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Venue", id);

            venue.IsActive = false;
            venue.UpdatedAt = DateTime.UtcNow;
            await _venueRepository.UpdateAsync(venue);
            await _auditLog.LogAsync("Delete", "Venue", id.ToString(), oldValues: $"{{\"name\":\"{venue.Name}\"}}");
        }

        public static VenueResponseDto MapToResponse(Venue v) => new()
        {
            Id = v.Id,
            Name = v.Name,
            Address = v.Address,
            City = v.City,
            State = v.State,
            Country = v.Country,
            ZipCode = v.ZipCode,
            Capacity = v.Capacity,
            Description = v.Description,
            ContactEmail = v.ContactEmail,
            ContactPhone = v.ContactPhone,
            IsActive = v.IsActive,
            CreatedAt = v.CreatedAt
        };
    }
}
