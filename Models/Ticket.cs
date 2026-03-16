namespace EventCalendarAPI.Models
{
    public enum TicketStatus { Reserved, Confirmed, Cancelled, Attended }
    public enum TicketType { Free, Paid, VIP }

    public class Ticket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public TicketType Type { get; set; } = TicketType.Free;
        public TicketStatus Status { get; set; } = TicketStatus.Reserved;
        public decimal Price { get; set; } = 0;
        public int Quantity { get; set; } = 1;
        public string? QrCode { get; set; }
        public string? SeatNumber { get; set; }
        public bool CheckedIn { get; set; } = false;
        public DateTime? CheckInTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        public int EventId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Event Event { get; set; } = null!;
        public User User { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
