using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Application.Events;
using LeadAssignment.Infrastructure.Data;
using LeadAssignment.Domain.Entities;
using Microsoft.Extensions.Logging;
using LeadAssignment.Application.Common.Interfaces;


namespace LeadAssignment.Infrastructure.Services
{
    public class DevEmailSender : IEmailSender
    {
        private readonly ILogger<DevEmailSender> _logger;

        public DevEmailSender(ILogger<DevEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default)
        {
            // Simulate sending email by logging to console with a clear format
            _logger.LogInformation(@"
======================================================
[DEV EMAIL SENDER] 
TO: {ToEmail}
SUBJECT: {Subject}
------------------------------------------------------
{Message}
======================================================", toEmail, subject, htmlMessage);

            return Task.CompletedTask;
        }
    }
}


