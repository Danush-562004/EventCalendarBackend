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

        public EventService(IEventRepository eventRepository, ICategoryRepository categoryRepository)
        {
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
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
            var items = await _eventRepository.SearchAsync(filter.Keyword, filter.CategoryId,
                filter.StartDate, filter.EndDate, filter.Privacy, filter.Page, filter.PageSize);
            var total = await _eventRepository.GetSearchCountAsync(filter.Keyword, filter.CategoryId,
                filter.StartDate, filter.EndDate, filter.Privacy);

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

            var ev = new Event
            {
                Title = request.Title,
                Description = request.Description,
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                Location = request.Location,
                //Privacy = request.Privacy,
                //IsAllDay = request.IsAllDay,
                ReminderEnabled = request.ReminderEnabled,
                ReminderMinutesBefore = request.ReminderMinutesBefore,
                //Recurrence = request.Recurrence,
                //RecurrenceRule = request.RecurrenceRule,
                MaxAttendees = request.MaxAttendees,
                CategoryId = request.CategoryId,
                VenueId = request.VenueId,
                UserId = userId
            };

            await _eventRepository.AddAsync(ev);
            var created = await _eventRepository.GetByIdWithDetailsAsync(ev.Id)
                ?? throw new Exception("Failed to retrieve created event.");
            return MapToResponse(created);
        }

        public async Task<EventResponseDto> UpdateAsync(int id, UpdateEventRequestDto request, int userId)
        {
            var ev = await _eventRepository.GetByIdWithDetailsAsync(id)
                ?? throw new EntityNotFoundException("Event", id);

            if (ev.UserId != userId)
                throw new UnauthorizedException("You can only update your own events.");

            if (request.Title != null) ev.Title = request.Title;
            if (request.Description != null) ev.Description = request.Description;
            if (request.StartDateTime.HasValue) ev.StartDateTime = request.StartDateTime.Value;
            if (request.EndDateTime.HasValue) ev.EndDateTime = request.EndDateTime.Value;
            if (request.Location != null) ev.Location = request.Location;
            //if (request.Privacy.HasValue) ev.Privacy = request.Privacy.Value;
            //if (request.IsAllDay.HasValue) ev.IsAllDay = request.IsAllDay.Value;
            if (request.ReminderEnabled.HasValue) ev.ReminderEnabled = request.ReminderEnabled.Value;
            if (request.ReminderMinutesBefore.HasValue) ev.ReminderMinutesBefore = request.ReminderMinutesBefore;
            //if (request.Recurrence.HasValue) ev.Recurrence = request.Recurrence.Value;
            //if (request.RecurrenceRule != null) ev.RecurrenceRule = request.RecurrenceRule;
            if (request.MaxAttendees.HasValue) ev.MaxAttendees = request.MaxAttendees.Value;
            if (request.CategoryId.HasValue) ev.CategoryId = request.CategoryId.Value;
            if (request.VenueId.HasValue) ev.VenueId = request.VenueId;
            ev.UpdatedAt = DateTime.UtcNow;

            if (ev.StartDateTime >= ev.EndDateTime)
                throw new ValidationException("Start date/time must be before end date/time.");

            await _eventRepository.UpdateAsync(ev);
            var updated = await _eventRepository.GetByIdWithDetailsAsync(ev.Id)!;
            return MapToResponse(updated!);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var ev = await _eventRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Event", id);

            if (ev.UserId != userId)
                throw new UnauthorizedException("You can only delete your own events.");

            ev.IsActive = false;
            ev.UpdatedAt = DateTime.UtcNow;
            await _eventRepository.UpdateAsync(ev);
        }

        public static EventResponseDto MapToResponse(Event e) => new()
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            StartDateTime = e.StartDateTime,
            EndDateTime = e.EndDateTime,
            Location = e.Location,
            //Privacy = e.Privacy.ToString(),
            //IsAllDay = e.IsAllDay,
            ReminderEnabled = e.ReminderEnabled,
            ReminderMinutesBefore = e.ReminderMinutesBefore,
            //Recurrence = e.Recurrence.ToString(),
            //RecurrenceRule = e.RecurrenceRule,
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

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
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
            return MapToResponse(category);
        }

        public async Task<CategoryResponseDto> UpdateAsync(int id, UpdateCategoryRequestDto request)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Category", id);

            if (request.Name != null) category.Name = request.Name;
            if (request.Description != null) category.Description = request.Description;
            if (request.ColorCode != null) category.ColorCode = request.ColorCode;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);
            return MapToResponse(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Category", id);

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.UpdateAsync(category);
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

        public VenueService(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<VenueResponseDto> GetByIdAsync(int id)
        {
            var venue = await _venueRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Venue", id);
            return MapToResponse(venue);
        }

        public async Task<PagedResponseDto<VenueResponseDto>> GetAllAsync(int page, int pageSize)
        {
            var result = await _venueRepository.GetPagedAsync(page, pageSize);
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
            return MapToResponse(venue);
        }

        public async Task<VenueResponseDto> UpdateAsync(int id, UpdateVenueRequestDto request)
        {
            var venue = await _venueRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Venue", id);

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
            return MapToResponse(venue);
        }

        public async Task DeleteAsync(int id)
        {
            var venue = await _venueRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Venue", id);

            venue.IsActive = false;
            venue.UpdatedAt = DateTime.UtcNow;
            await _venueRepository.UpdateAsync(venue);
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
