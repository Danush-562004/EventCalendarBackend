using EventCalendarAPI.Data;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCalendarAPI.Services
{
    // ─── Refund Service ──────────────────────────────────────────
    public class RefundService : IRefundService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuditLogService _auditLog;

        public RefundService(ApplicationDbContext context,
            INotificationRepository notificationRepository,
            IAuditLogService auditLog)
        {
            _context = context;
            _notificationRepository = notificationRepository;
            _auditLog = auditLog;
        }

        public async Task ProcessRefundsForEventAsync(int eventId, string eventTitle, DateTime eventStartDateTime)
        {
            // Determine refund percentage based on how far the cancellation is from the event start
            var hoursUntilStart = (eventStartDateTime - DateTime.UtcNow).TotalHours;
            var refundPercent = hoursUntilStart > 48 ? 1.0m : 0.5m;
            var refundLabel = refundPercent == 1.0m ? "100%" : "50%";

            // Get all confirmed tickets for this event with their completed payments
            var tickets = await _context.Tickets
                .Include(t => t.Payments)
                .Where(t => t.EventId == eventId && t.Status == TicketStatus.Confirmed)
                .ToListAsync();

            foreach (var ticket in tickets)
            {
                var completedPayments = ticket.Payments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .ToList();

                foreach (var payment in completedPayments)
                {
                    var refundAmount = Math.Round(payment.Amount * refundPercent, 2);

                    // Mark original payment as refunded
                    payment.Status = PaymentStatus.Refunded;
                    payment.Notes = $"{refundLabel} refund issued — event '{eventTitle}' was cancelled.";
                    payment.UpdatedAt = DateTime.UtcNow;

                    // Create a refund payment record
                    var refundPayment = new Payment
                    {
                        TicketId = ticket.Id,
                        Amount = refundAmount,
                        Currency = payment.Currency,
                        Method = payment.Method,
                        Status = PaymentStatus.Refunded,
                        TransactionId = $"REFUND-{payment.TransactionId ?? payment.Id.ToString()}",
                        Notes = $"{refundLabel} refund for cancelled event '{eventTitle}'.",
                        PaymentDate = DateTime.UtcNow
                    };
                    _context.Payments.Add(refundPayment);

                    // Notify the user
                    await _notificationRepository.AddAsync(new Notification
                    {
                        UserId = ticket.UserId,
                        Title = "Refund Issued",
                        Message = $"The event \"{eventTitle}\" was cancelled. You have received a {refundLabel} refund of {payment.Currency} {refundAmount:F2} for ticket {ticket.TicketNumber}."
                    });

                    await _auditLog.LogAsync("Refund", "Payment", payment.Id.ToString(),
                        newValues: $"{{\"refundAmount\":{refundAmount},\"refundPercent\":\"{refundLabel}\",\"eventId\":{eventId},\"ticketId\":{ticket.Id}}}");
                }

                // Cancel the ticket
                ticket.Status = TicketStatus.Cancelled;
                ticket.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }

    // ─── Notification Service ────────────────────────────────────
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetMyNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            return notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            });
        }

        public async Task MarkAllReadAsync(int userId) =>
            await _notificationRepository.MarkAllReadAsync(userId);

        public async Task<int> GetUnreadCountAsync(int userId) =>
            await _notificationRepository.GetUnreadCountAsync(userId);
    }
}
