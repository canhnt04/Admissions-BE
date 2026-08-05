using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace LeadAssignment.Infrastructure.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSettings> emailSettings, ILogger<SmtpEmailSender> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                var finalToEmail = string.IsNullOrWhiteSpace(_emailSettings.DevEmailOverride)
                    ? toEmail
                    : _emailSettings.DevEmailOverride;

                using var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = _emailSettings.UseSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromAddress, _emailSettings.FromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(finalToEmail);

                // Add original recipient to CC or Body if it was overridden
                if (!string.IsNullOrWhiteSpace(_emailSettings.DevEmailOverride) && toEmail != _emailSettings.DevEmailOverride)
                {
                    mailMessage.Body = $"<div style='background: #fff3cd; color: #856404; padding: 10px; margin-bottom: 20px; border: 1px solid #ffeeba;'><b>[DEV OVERRIDE]</b> Original recipient: {toEmail}</div>" + mailMessage.Body;
                }

                await client.SendMailAsync(mailMessage, cancellationToken);

                _logger.LogInformation("Đã gửi email SMTP thành công tới {ToEmail} (Gốc: {OriginalToEmail}). Tiêu đề: {Subject}", finalToEmail, toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email SMTP tới {ToEmail}", toEmail);
                // Optionally throw or fail silently based on your error handling policy
            }
        }
    }
}
