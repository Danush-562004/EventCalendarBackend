using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendarAPI.Controllers
{
    [Authorize]
    public class EventsController : BaseController
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>Get all events with pagination.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<EventResponseDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _eventService.GetAllAsync(page, pageSize);
            return Ok(ApiResponseDto<PagedResponseDto<EventResponseDto>>.Ok(result));
        }

        /// <summary>Search and filter events with pagination.</summary>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<EventResponseDto>>), 200)]
        public async Task<IActionResult> Search([FromQuery] EventFilterRequestDto filter)
        {
            var result = await _eventService.SearchAsync(filter);
            return Ok(ApiResponseDto<PagedResponseDto<EventResponseDto>>.Ok(result));
        }

        /// <summary>Get an event by ID.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponseDto<EventResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var ev = await _eventService.GetByIdAsync(id);
            return Ok(ApiResponseDto<EventResponseDto>.Ok(ev));
        }

        /// <summary>Get all events created by the current user.</summary>
        [HttpGet("my-events")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<EventResponseDto>>), 200)]
        public async Task<IActionResult> GetMyEvents()
        {
            var events = await _eventService.GetByCurrentUserAsync(GetCurrentUserId());
            return Ok(ApiResponseDto<IEnumerable<EventResponseDto>>.Ok(events));
        }

        /// <summary>Create a new event.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<EventResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Create([FromBody] CreateEventRequestDto request)
        {
            var ev = await _eventService.CreateAsync(request, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id = ev.Id },
                ApiResponseDto<EventResponseDto>.Ok(ev, "Event created successfully."));
        }

        /// <summary>Update an existing event.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponseDto<EventResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateEventRequestDto request)
        {
            var ev = await _eventService.UpdateAsync(id, request, GetCurrentUserId());
            return Ok(ApiResponseDto<EventResponseDto>.Ok(ev, "Event updated successfully."));
        }

        /// <summary>Delete (soft-delete) an event.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _eventService.DeleteAsync(id, GetCurrentUserId());
            return Ok(ApiResponseDto<object>.Ok(null!, "Event deleted successfully."));
        }
    }
}
