using System.ComponentModel.DataAnnotations;
using EventCalendarAPI.Models;

namespace EventCalendarAPI.DTOs.Request
{
    // ─── Category ────────────────────────────────────────────────
    public class CreateCategoryRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "ColorCode must be a valid hex color (e.g. #3498db)")]
        public string ColorCode { get; set; } = "#3498db";
    }

    public class UpdateCategoryRequestDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "ColorCode must be a valid hex color")]
        public string? ColorCode { get; set; }
    }

    // ─── Venue ───────────────────────────────────────────────────
    public class CreateVenueRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        public string? ZipCode { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        public string? Description { get; set; }

        [EmailAddress]
        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }
    }

    public class UpdateVenueRequestDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }

        [Range(1, int.MaxValue)]
        public int? Capacity { get; set; }

        public string? Description { get; set; }

        [EmailAddress]
        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }
    }

    // ─── Ticket ──────────────────────────────────────────────────
    public class CreateTicketRequestDto
    {
        [Required]
        public int EventId { get; set; }
        

        public TicketType Type { get; set; } = TicketType.Free;


        //[Range(0, double.MaxValue)]
        //public decimal Price { get; set; } = 0;

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        public string? SeatNumber { get; set; }
    }

    public class UpdateTicketRequestDto
    {
        public TicketStatus? Status { get; set; }
        public bool? CheckedIn { get; set; }
        public string? SeatNumber { get; set; }
    }

    // ─── Payment ─────────────────────────────────────────────────
    public class CreatePaymentRequestDto
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }         

        public string Currency { get; set; } = "USD";

        [Required]
        public string Method { get; set; } = string.Empty;

        public string? TransactionId { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdatePaymentRequestDto
    {
        public PaymentStatus? Status { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentGatewayReference { get; set; }
        public string? Notes { get; set; }
    }

    // ─── User Profile ─────────────────────────────────────────────
    public class UpdateUserRequestDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ProfilePicture { get; set; }

        public bool? EmailNotifications { get; set; }

        public bool? PushNotifications { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
