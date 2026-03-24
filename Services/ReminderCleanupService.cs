using EventCalendarAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace EventCalendarAPI.Services
{
    /// <summary>
    /// Background service that soft-deletes reminders whose associated event has ended.
    /// Runs every 30 minutes.
    /// </summary>
    public class ReminderCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderCleanupService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

        public ReminderCleanupService(IServiceScopeFactory scopeFactory, ILogger<ReminderCleanupService> logger)
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
                    await CleanupExpiredRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during reminder cleanup.");
                }
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task CleanupExpiredRemindersAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            // Find active reminders whose event has already ended
            var expired = await db.Reminders
                .Include(r => r.Event)
                .Where(r => r.IsActive && r.Event != null && r.Event.EndDateTime <= now)
                .ToListAsync();

            if (expired.Count == 0) return;

            foreach (var r in expired)
            {
                r.IsActive = false;
                r.UpdatedAt = now;
            }

            await db.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {Count} expired reminders.", expired.Count);
        }
    }
}
