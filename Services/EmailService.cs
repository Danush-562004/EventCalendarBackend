using EventCalendarAPI.Interfaces;
using System.Net;
using System.Net.Mail;

namespace EventCalendarAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var host     = _config["Email:SmtpHost"]!;
            var port     = int.Parse(_config["Email:SmtpPort"]!);
            var from     = _config["Email:SenderEmail"]!;
            var fromName = _config["Email:SenderName"]!;
            var password = _config["Email:Password"]!;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(from, password),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From       = new MailAddress(from, fromName),
                Subject    = subject,
                Body       = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email} — {Subject}", toEmail, subject);
        }
    }
}
