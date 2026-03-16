namespace EventCalendarAPI.Models
{
    public enum ReminderType { Email, Push, Both }

    public class Reminder
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public ReminderType Type { get; set; } = ReminderType.Email;
        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        public int EventId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Event Event { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
