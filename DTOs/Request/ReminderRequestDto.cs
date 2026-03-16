using System.ComponentModel.DataAnnotations;
using EventCalendarAPI.Models;

namespace EventCalendarAPI.DTOs.Request
{
    public class CreateReminderRequestDto
    {
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        public string? Message { get; set; }

        [Required]
        public DateTime ReminderDateTime { get; set; }

        public ReminderType Type { get; set; } = ReminderType.Email;

        [Required]
        public int EventId { get; set; }
    }

    public class UpdateReminderRequestDto
    {
        [MaxLength(300)]
        public string? Title { get; set; }

        public string? Message { get; set; }

        public DateTime? ReminderDateTime { get; set; }

        public ReminderType? Type { get; set; }
    }
}
