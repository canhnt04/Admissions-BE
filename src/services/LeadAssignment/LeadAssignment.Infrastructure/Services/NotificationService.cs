using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Application.Events;
using LeadAssignment.Infrastructure.Data;
using LeadAssignment.Domain.Entities;



using Microsoft.Extensions.Logging;

namespace LeadAssignment.Infrastructure.Services
{
    /// <summary>
    /// Implementation gửi notification in-app.
    /// Lưu notification vào DB — frontend sẽ poll hoặc dùng SignalR để hiển thị.
    /// (Có thể mở rộng thêm email/Zalo notification sau)
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly AssignmentDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(AssignmentDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task NotifyLeadAssignedAsync(Guid recipientId, Guid customerId, string customerName, CancellationToken cancellationToken = default)
        {
            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = recipientId,
                Type = NotificationType.LeadAssigned,
                Title = "Lead mới được giao",
                Message = $"Bạn được giao khách hàng mới: {customerName}. Vui lòng liên hệ trong 30 phút.",
                ReferenceId = customerId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Notification sent: LeadAssigned to {RecipientId} for customer {CustomerName}", recipientId, customerName);
        }

        public async Task NotifySlaWarningAsync(Guid recipientId, Guid customerId, string customerName, DateTime deadline, CancellationToken cancellationToken = default)
        {
            var remaining = deadline - DateTime.UtcNow;

            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = recipientId,
                Type = NotificationType.SlaWarning,
                Title = "⚠️ SLA sắp hết hạn",
                Message = $"Bạn còn {(int)remaining.TotalMinutes} phút để liên hệ khách hàng: {customerName}. Hãy upload bằng chứng liên hệ ngay!",
                ReferenceId = customerId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Notification sent: SlaWarning to {RecipientId}, remaining {Minutes}m", recipientId, (int)remaining.TotalMinutes);
        }

        public async Task NotifySlaViolationAsync(Guid recipientId, Guid customerId, string customerName, CancellationToken cancellationToken = default)
        {
            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = recipientId,
                Type = NotificationType.SlaViolation,
                Title = "🚫 Vi phạm SLA — Lead bị thu hồi",
                Message = $"Khách hàng {customerName} đã bị thu hồi do bạn không liên hệ trong 30 phút.",
                ReferenceId = customerId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Notification sent: SlaViolation to {RecipientId} for customer {CustomerName}", recipientId, customerName);
        }

        public async Task NotifyLeadReassignedAsync(Guid recipientId, Guid customerId, string customerName, string reason, CancellationToken cancellationToken = default)
        {
            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = recipientId,
                Type = NotificationType.LeadReassigned,
                Title = "Lead được giao lại cho bạn",
                Message = $"Bạn nhận được khách hàng: {customerName}. Lý do: {reason}. Vui lòng liên hệ trong 30 phút.",
                ReferenceId = customerId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Notification sent: LeadReassigned to {RecipientId} for customer {CustomerName}", recipientId, customerName);
        }
    }
}


