using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendarAPI.Controllers
{
    // ─── Categories Controller ───────────────────────────────────
    [Authorize]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var categories = await _categoryService.GetAllAsync(page, pageSize);
            return Ok(ApiResponseDto<PagedResponseDto<CategoryResponseDto>>.Ok(categories));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(ApiResponseDto<CategoryResponseDto>.Ok(category));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto request)
        {
            var category = await _categoryService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = category.Id },
                ApiResponseDto<CategoryResponseDto>.Ok(category, "Category created successfully."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCategoryRequestDto request)
        {
            var category = await _categoryService.UpdateAsync(id, request);
            return Ok(ApiResponseDto<CategoryResponseDto>.Ok(category, "Category updated successfully."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _categoryService.DeleteAsync(id);
            return Ok(ApiResponseDto<object>.Ok(null!, "Category deleted successfully."));
        }
    }

    // ─── Venues Controller ───────────────────────────────────────
    [Authorize]
    public class VenuesController : BaseController
    {
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] string? city = null, [FromQuery] string? country = null)
        {
            var venues = await _venueService.GetAllAsync(page, pageSize, city, country);
            return Ok(ApiResponseDto<PagedResponseDto<VenueResponseDto>>.Ok(venues));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var venue = await _venueService.GetByIdAsync(id);
            return Ok(ApiResponseDto<VenueResponseDto>.Ok(venue));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateVenueRequestDto request)
        {
            var venue = await _venueService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = venue.Id },
                ApiResponseDto<VenueResponseDto>.Ok(venue, "Venue created successfully."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateVenueRequestDto request)
        {
            var venue = await _venueService.UpdateAsync(id, request);
            return Ok(ApiResponseDto<VenueResponseDto>.Ok(venue, "Venue updated successfully."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _venueService.DeleteAsync(id);
            return Ok(ApiResponseDto<object>.Ok(null!, "Venue deleted successfully."));
        }
    }

    // ─── Tickets Controller ──────────────────────────────────────
    [Authorize]
    public class TicketsController : BaseController
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
        {
            var tickets = await _ticketService.GetAllAsync(page, pageSize, status);
            return Ok(ApiResponseDto<PagedResponseDto<TicketResponseDto>>.Ok(tickets));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponseDto<TicketResponseDto>.Ok(ticket));
        }

        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var tickets = await _ticketService.GetByCurrentUserAsync(GetCurrentUserId());
            return Ok(ApiResponseDto<IEnumerable<TicketResponseDto>>.Ok(tickets));
        }

        [HttpGet("by-event/{eventId:int}")]
        public async Task<IActionResult> GetByEvent([FromRoute] int eventId)
        {
            var tickets = await _ticketService.GetByEventIdAsync(eventId, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponseDto<IEnumerable<TicketResponseDto>>.Ok(tickets));
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequestDto request)
        {
            var ticket = await _ticketService.CreateAsync(request, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id },
                ApiResponseDto<TicketResponseDto>.Ok(ticket, "Ticket booked successfully."));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateTicketRequestDto request)
        {
            var ticket = await _ticketService.UpdateAsync(id, request, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponseDto<TicketResponseDto>.Ok(ticket, "Ticket updated successfully."));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _ticketService.DeleteAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(ApiResponseDto<object>.Ok(null!, "Ticket cancelled successfully."));
        }
    }

    // ─── Payments Controller ─────────────────────────────────────
    [Authorize]
    public class PaymentsController : BaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
        {
            var payments = await _paymentService.GetAllAsync(page, pageSize, status);
            return Ok(ApiResponseDto<PagedResponseDto<PaymentResponseDto>>.Ok(payments));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            return Ok(ApiResponseDto<PaymentResponseDto>.Ok(payment));
        }

        [HttpGet("by-ticket/{ticketId:int}")]
        public async Task<IActionResult> GetByTicket([FromRoute] int ticketId)
        {
            var payments = await _paymentService.GetByTicketIdAsync(ticketId);
            return Ok(ApiResponseDto<IEnumerable<PaymentResponseDto>>.Ok(payments));
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequestDto request)
        {
            var payment = await _paymentService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id },
                ApiResponseDto<PaymentResponseDto>.Ok(payment, "Payment processed successfully."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdatePaymentRequestDto request)
        {
            var payment = await _paymentService.UpdateAsync(id, request);
            return Ok(ApiResponseDto<PaymentResponseDto>.Ok(payment, "Payment updated successfully."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _paymentService.DeleteAsync(id);
            return Ok(ApiResponseDto<object>.Ok(null!, "Payment deleted successfully."));
        }
    }

    // ─── Audit Logs Controller ───────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : BaseController
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? action = null,
            [FromQuery] string? entityType = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var result = await _auditLogService.GetAllAsync(page, pageSize, action, entityType, from, to);
            return Ok(ApiResponseDto<PagedResponseDto<AuditLogResponseDto>>.Ok(result));
        }
    }
}
