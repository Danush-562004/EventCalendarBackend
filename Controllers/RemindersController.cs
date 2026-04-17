using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendarAPI.Controllers
{
    [Authorize]
    public class RemindersController : BaseController
    {
        private readonly IReminderService _reminderService;

        public RemindersController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        // Get all reminders for the current user with pagination.
        [HttpGet]
        [HttpGet("my")]
        [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<ReminderResponseDto>>), 200)]
        public async Task<IActionResult> GetMyReminders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _reminderService.GetByCurrentUserAsync(GetCurrentUserId(), page, pageSize);
            return Ok(ApiResponseDto<PagedResponseDto<ReminderResponseDto>>.Ok(result));
        }

        // Get a reminder by ID.
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<ReminderResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var reminder = await _reminderService.GetByIdAsync(id, GetCurrentUserId());
            return Ok(ApiResponseDto<ReminderResponseDto>.Ok(reminder));
        }

        // Get reminders for a specific event.
        [HttpGet("by-event/{eventId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<ReminderResponseDto>>), 200)]
        public async Task<IActionResult> GetByEvent([FromRoute] int eventId)
        {
            var reminders = await _reminderService.GetByEventIdAsync(eventId, GetCurrentUserId());
            return Ok(ApiResponseDto<IEnumerable<ReminderResponseDto>>.Ok(reminders));
        }

        // Create a new reminder.
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<ReminderResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> Create([FromBody] CreateReminderRequestDto request)
        {
            var reminder = await _reminderService.CreateAsync(request, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id = reminder.Id },
                ApiResponseDto<ReminderResponseDto>.Ok(reminder, "Reminder created successfully."));
        }

        // Update a reminder.
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<ReminderResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateReminderRequestDto request)
        {
            var reminder = await _reminderService.UpdateAsync(id, request, GetCurrentUserId());
            return Ok(ApiResponseDto<ReminderResponseDto>.Ok(reminder, "Reminder updated successfully."));
        }

        // Delete (soft-delete) a reminder.
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _reminderService.DeleteAsync(id, GetCurrentUserId());
            return Ok(ApiResponseDto<object>.Ok(null!, "Reminder deleted successfully."));
        }
    }
}
