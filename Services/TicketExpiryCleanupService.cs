using EventCalendarAPI.Data;
using EventCalendarAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCalendarAPI.Services
{
    /// <summary>
    /// Background service that cancels Reserved tickets whose 5-minute payment
    /// window has expired and no completed payment exists.
    /// Runs every 60 seconds. Cancelling the ticket frees the seats automatically
    /// because AvailableSeats = MaxAttendees - sum(non-cancelled ticket quantities).
    /// </summary>
    public class TicketExpiryCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TicketExpiryCleanupService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
        private const int PaymentWindowMinutes = 5;

        public TicketExpiryCleanupService(IServiceScopeFactory scopeFactory,
            ILogger<TicketExpiryCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CancelExpiredTicketsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during ticket expiry cleanup.");
                }
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task CancelExpiredTicketsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cutoff = DateTime.UtcNow.AddMinutes(-PaymentWindowMinutes);

            // Find Reserved tickets created more than 5 minutes ago
            // that have no Completed payment
            var expired = await db.Tickets
                .Include(t => t.Payments)
                .Where(t =>
                    t.Status == TicketStatus.Reserved &&
                    t.CreatedAt <= cutoff &&
                    !t.Payments.Any(p => p.Status == PaymentStatus.Completed))
                .ToListAsync();

            if (expired.Count == 0) return;

            var now = DateTime.UtcNow;
            foreach (var ticket in expired)
            {
                ticket.Status = TicketStatus.Cancelled;
                ticket.UpdatedAt = now;
            }

            await db.SaveChangesAsync();
            _logger.LogInformation(
                "Expired {Count} unpaid ticket(s) — seats released.", expired.Count);
        }
    }
}
