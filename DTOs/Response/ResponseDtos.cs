using EventCalendarAPI.Models;

namespace EventCalendarAPI.DTOs.Response
{
    // ─── Generic ─────────────────────────────────────────────────
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponseDto<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Message = message, Data = data };

        public static ApiResponseDto<T> Fail(string message, List<string>? errors = null) =>
            new() { Success = false, Message = message, Errors = errors };
    }

    public class PagedResponseDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    // ─── Auth ────────────────────────────────────────────────────
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
        public UserResponseDto User { get; set; } = null!;
    }

    // ─── User ────────────────────────────────────────────────────
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ─── AuditLog ────────────────────────────────────────────────
    public class AuditLogResponseDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // ─── Category ────────────────────────────────────────────────
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ─── Venue ───────────────────────────────────────────────────
    public class VenueResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? ZipCode { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ─── Event ───────────────────────────────────────────────────
    public class EventResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int AvailableSeats { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? Location { get; set; }
        //public string Privacy { get; set; } = string.Empty;
        //public bool IsAllDay { get; set; }
        public bool ReminderEnabled { get; set; }
        public int? ReminderMinutesBefore { get; set; }
        //public string Recurrence { get; set; } = string.Empty;
        //public string? RecurrenceRule { get; set; }
        public int MaxAttendees { get; set; }
        public int TicketCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UserId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public CategoryResponseDto Category { get; set; } = null!;
        public VenueResponseDto? Venue { get; set; }
    }

    // ─── Payment ─────────────────────────────────────────────────
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        //public int Quantity { get; set; }
        //public decimal TotalPrice => Quantity * Amount;
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TicketId { get; set; }
    }

    // ─── Ticket ──────────────────────────────────────────────────
    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? SeatNumber { get; set; }
        public bool CheckedIn { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PaymentDeadline { get; set; }  // CreatedAt + 5 minutes
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventEndDateTime { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public List<PaymentResponseDto> Payments { get; set; } = new();
    }

    // ─── Reminder ────────────────────────────────────────────────
    public class ReminderResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    // ─── Notification ─────────────────────────────────────────────
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
