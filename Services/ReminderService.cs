using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;

namespace EventCalendarAPI.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IReminderRepository _reminderRepository;
        private readonly IEventRepository _eventRepository;

        public ReminderService(IReminderRepository reminderRepository, IEventRepository eventRepository)
        {
            _reminderRepository = reminderRepository;
            _eventRepository = eventRepository;
        }

        public async Task<ReminderResponseDto> GetByIdAsync(int id, int requestingUserId)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Reminder", id);

            if (reminder.UserId != requestingUserId)
                throw new UnauthorizedException("You can only view your own reminders.");

            return MapToResponse(reminder);
        }

        public async Task<PagedResponseDto<ReminderResponseDto>> GetByCurrentUserAsync(int userId, int page, int pageSize)
        {
            var result = await _reminderRepository.GetPagedByUserAsync(userId, page, pageSize);
            return new PagedResponseDto<ReminderResponseDto>
            {
                Items = result.Items.Select(MapToResponse).ToList(),
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<ReminderResponseDto>> GetByEventIdAsync(int eventId, int requestingUserId)
        {
            var ev = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new EntityNotFoundException("Event", eventId);

            if (ev.UserId != requestingUserId)
                throw new UnauthorizedException("You can only view reminders for your own events.");

            var reminders = await _reminderRepository.GetByEventIdAsync(eventId);
            return reminders.Select(MapToResponse);
        }

        public async Task<ReminderResponseDto> CreateAsync(CreateReminderRequestDto request, int userId)
        {
            var ev = await _eventRepository.GetByIdAsync(request.EventId)
                ?? throw new EntityNotFoundException("Event", request.EventId);

            if (request.ReminderDateTime >= ev.StartDateTime == false && request.ReminderDateTime > ev.StartDateTime)
                throw new ValidationException("Reminder date/time must be before the event start time.");

            var reminder = new Reminder
            {
                Title = request.Title,
                Message = request.Message,
                ReminderDateTime = request.ReminderDateTime,
                Type = request.Type,
                EventId = request.EventId,
                UserId = userId
            };

            await _reminderRepository.AddAsync(reminder);

            // Reload with event details
            var created = await _reminderRepository.GetByIdAsync(reminder.Id)!;
            created!.Event = ev;
            return MapToResponse(created);
        }

        public async Task<ReminderResponseDto> UpdateAsync(int id, UpdateReminderRequestDto request, int requestingUserId)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Reminder", id);

            if (reminder.UserId != requestingUserId)
                throw new UnauthorizedException("You can only update your own reminders.");

            if (request.Title != null) reminder.Title = request.Title;
            if (request.Message != null) reminder.Message = request.Message;
            if (request.ReminderDateTime.HasValue) reminder.ReminderDateTime = request.ReminderDateTime.Value;
            if (request.Type.HasValue) reminder.Type = request.Type.Value;
            reminder.UpdatedAt = DateTime.UtcNow;

            await _reminderRepository.UpdateAsync(reminder);
            return MapToResponse(reminder);
        }

        public async Task DeleteAsync(int id, int requestingUserId)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("Reminder", id);

            if (reminder.UserId != requestingUserId)
                throw new UnauthorizedException("You can only delete your own reminders.");

            reminder.IsActive = false;
            reminder.UpdatedAt = DateTime.UtcNow;
            await _reminderRepository.UpdateAsync(reminder);
        }

        private static ReminderResponseDto MapToResponse(Reminder r) => new()
        {
            Id = r.Id,
            Title = r.Title,
            Message = r.Message,
            ReminderDateTime = r.ReminderDateTime,
            Type = r.Type.ToString(),
            IsSent = r.IsSent,
            SentAt = r.SentAt,
            IsActive = r.IsActive,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            EventId = r.EventId,
            EventTitle = r.Event?.Title ?? string.Empty,
            UserId = r.UserId
        };
    }
}
