namespace EventCalendarAPI.Models
{
    public enum PaymentStatus { Pending, Completed, Failed, Refunded }
    public enum PaymentMethod { CreditCard, DebitCard, PayPal, BankTransfer, Cash }

    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentMethod Method { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentGatewayReference { get; set; }
        public string? Notes { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign key
        public int TicketId { get; set; }

        // Navigation properties
        public Ticket Ticket { get; set; } = null!;
    }
}
