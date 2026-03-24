namespace EventCalendarAPI.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;       // e.g. "Create", "Update", "Delete", "Login", "Register"
        public string EntityType { get; set; } = string.Empty;   // e.g. "Event", "Venue", "Category"
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }                   // JSON string
        public string? NewValues { get; set; }                   // JSON string
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
