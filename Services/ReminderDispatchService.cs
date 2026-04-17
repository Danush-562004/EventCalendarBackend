using EventCalendarAPI.Data;
using EventCalendarAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventCalendarAPI.Services
{
    
    /// Background service that runs every minute, finds due reminders,
    /// sends an email to the user, and marks the reminder as sent.
    
    public class ReminderDispatchService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderDispatchService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        public ReminderDispatchService(IServiceScopeFactory scopeFactory, ILogger<ReminderDispatchService> logger)
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
                    await DispatchDueRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during reminder dispatch.");
                }
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task DispatchDueRemindersAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db           = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var now = DateTime.UtcNow;

            var due = await db.Reminders
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.IsActive && !r.IsSent && r.ReminderDateTime <= now)
                .ToListAsync();

            if (due.Count == 0) return;

            foreach (var reminder in due)
            {
                try
                {
                    var subject  = $"⏰ Reminder: {reminder.Event.Title} is starting soon!";
                    var htmlBody = BuildEmailBody(reminder.User.FirstName, reminder.Event.Title,
                                                  reminder.Event.StartDateTime, reminder.Title,
                                                  reminder.Message);

                    await emailService.SendAsync(reminder.User.Email, reminder.User.FirstName, subject, htmlBody);

                    reminder.IsSent  = true;
                    reminder.SentAt  = now;
                    reminder.UpdatedAt = now;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder {ReminderId} to {Email}.",
                        reminder.Id, reminder.User.Email);
                }
            }

            await db.SaveChangesAsync();
            _logger.LogInformation("Dispatched {Count} reminder(s).", due.Count(r => r.IsSent));
        }

        private static string BuildEmailBody(string firstName, string eventTitle,
            DateTime startDateTime, string reminderTitle, string? message)
        {
            var localTime = startDateTime.ToString("dddd, MMMM d yyyy 'at' h:mm tt") + " UTC";
            var customMsg = string.IsNullOrWhiteSpace(message)
                ? string.Empty
                : $"<p style='color:#555;font-size:15px;'>{message}</p>";

            return $"""
                <!DOCTYPE html>
                <html>
                <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;'>
                  <table width='100%' cellpadding='0' cellspacing='0'>
                    <tr><td align='center' style='padding:40px 0;'>
                      <table width='560' cellpadding='0' cellspacing='0'
                             style='background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08);'>
                        <tr>
                          <td style='background:#4f46e5;padding:32px 40px;text-align:center;'>
                            <h1 style='margin:0;color:#ffffff;font-size:24px;'>⏰ Event Reminder</h1>
                          </td>
                        </tr>
                        <tr>
                          <td style='padding:36px 40px;'>
                            <p style='font-size:16px;color:#333;'>Hi <strong>{firstName}</strong>,</p>
                            <p style='font-size:15px;color:#555;'>
                              This is your reminder for the upcoming event:
                            </p>
                            <div style='background:#f0f0ff;border-left:4px solid #4f46e5;
                                        border-radius:8px;padding:20px 24px;margin:20px 0;'>
                              <h2 style='margin:0 0 8px;color:#4f46e5;font-size:20px;'>{eventTitle}</h2>
                              <p style='margin:0;color:#555;font-size:14px;'>📅 {localTime}</p>
                            </div>
                            <p style='font-size:15px;color:#555;'><strong>Reminder:</strong> {reminderTitle}</p>
                            {customMsg}
                            <p style='font-size:13px;color:#999;margin-top:32px;'>
                              You're receiving this because you set a reminder on Event Calendar.
                            </p>
                          </td>
                        </tr>
                        <tr>
                          <td style='background:#f9f9f9;padding:16px 40px;text-align:center;'>
                            <p style='margin:0;font-size:12px;color:#aaa;'>© Event Calendar</p>
                          </td>
                        </tr>
                      </table>
                    </td></tr>
                  </table>
                </body>
                </html>
                """;
        }
    }
}
