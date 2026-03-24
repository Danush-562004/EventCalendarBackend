namespace EventCalendarAPI.Models
{
    public enum EventPrivacy { Public, Private, InviteOnly }
    public enum RecurrencePattern { None, Daily, Weekly, Monthly, Yearly, Custom }

    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public decimal Price { get; set; } = 0;

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? Location { get; set; }
        public EventPrivacy Privacy { get; set; } = EventPrivacy.Public;
        public bool IsAllDay { get; set; } = false;
        public bool ReminderEnabled { get; set; } = false;
        public int? ReminderMinutesBefore { get; set; }
        public RecurrencePattern Recurrence { get; set; } = RecurrencePattern.None;
        public string? RecurrenceRule { get; set; }
        public int MaxAttendees { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int? VenueId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public Venue? Venue { get; set; }
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }
}
