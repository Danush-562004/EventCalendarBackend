using System.ComponentModel.DataAnnotations;
using EventCalendarAPI.Models;

namespace EventCalendarAPI.DTOs.Request
{
    public class CreateEventRequestDto
    {
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; } = 0;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        public string? Location { get; set; }

        //public EventPrivacy Privacy { get; set; } = EventPrivacy.Public;

        //public bool IsAllDay { get; set; } = false;

        public bool ReminderEnabled { get; set; } = false;

        public int? ReminderMinutesBefore { get; set; }

        //public RecurrencePattern Recurrence { get; set; } = RecurrencePattern.None;

        //public string? RecurrenceRule { get; set; }

        public int MaxAttendees { get; set; } = 0;

        [Required]
        public int CategoryId { get; set; }

        public int? VenueId { get; set; }
    }

    public class UpdateEventRequestDto
    {
        [MaxLength(300)]
        public string? Title { get; set; }
        public decimal? Price { get; set; }

        public string? Description { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public string? Location { get; set; }

        //public EventPrivacy? Privacy { get; set; }

        //public bool? IsAllDay { get; set; }

        public bool? ReminderEnabled { get; set; }

        public int? ReminderMinutesBefore { get; set; }

       // public RecurrencePattern? Recurrence { get; set; }

        //public string? RecurrenceRule { get; set; }

        public int? MaxAttendees { get; set; }

        public int? CategoryId { get; set; }

        public int? VenueId { get; set; }
    }

    public class EventFilterRequestDto
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public int? VenueId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public EventPrivacy? Privacy { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
