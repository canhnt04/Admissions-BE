using Crm.Application.Common.Interfaces;
using Crm.Application.Events;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Services
{
    /// <summary>
    /// Background service kiểm tra SLA mỗi phút.
    /// SLA timeout = 30 phút. Nếu NV không liên hệ KH trong 30 phút → vi phạm SLA → thu hồi lead.
    /// 
    /// Logic:
    /// 1. Query SlaTracking WHERE IsContactMade = false AND Deadline &lt; Now AND IsReassigned = false AND IsViolated = false
    /// 2. Với mỗi violation → publish SlaViolationEvent
    /// 3. SlaViolationConsumer sẽ xử lý event này để giao lại lead
    /// 
    /// Cũng kiểm tra SLA sắp hết (còn 5 phút) để gửi cảnh báo.
    /// </summary>
    public class SlaMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SlaMonitorService> _logger;

        /// <summary>
        /// Chu kỳ kiểm tra SLA (1 phút) — vì SLA chỉ có 30 phút nên phải check thường xuyên
        /// </summary>
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Ngưỡng cảnh báo SLA sắp hết (5 phút trước deadline)
        /// </summary>
        private const int WARNING_THRESHOLD_MINUTES = 5;

        public SlaMonitorService(IServiceScopeFactory scopeFactory, ILogger<SlaMonitorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SlaMonitorService started. Check interval: {Interval}", CheckInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckSlaViolationsAsync(stoppingToken);
                    await CheckSlaWarningsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong SlaMonitorService");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }

            _logger.LogInformation("SlaMonitorService stopped.");
        }

        /// <summary>
        /// Kiểm tra và xử lý SLA violations (quá hạn 30 phút)
        /// </summary>
        private async Task CheckSlaViolationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var now = DateTime.UtcNow;

            // Tìm tất cả SLA đã quá hạn mà chưa xử lý
            var violations = await context.SlaTrackings
                .Include(s => s.Customer)
                .Include(s => s.Assignee)
                .Where(s => !s.IsContactMade && s.Deadline < now && !s.IsReassigned && !s.IsViolated)
                .ToListAsync(cancellationToken);

            if (violations.Count == 0) return;

            _logger.LogWarning("Phát hiện {Count} SLA violations", violations.Count);

            foreach (var sla in violations)
            {
                // Mark violated
                sla.IsViolated = true;

                // Ghi AuditLog
                context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = Domain.Entities.Action.SlaViolation,
                    Detail = $"SLA Violation: NV [{sla.Assignee?.FullName}] không liên hệ KH [{sla.Customer?.Name}] trong 30 phút. " +
                             $"Giao lúc: {sla.AssignedAt:HH:mm:ss}, Deadline: {sla.Deadline:HH:mm:ss}",
                    RecordId = sla.CustomerId,
                    RecordDesc = sla.Customer?.Name ?? "N/A",
                    RecordEntity = RecordEntity.SlaTracking,
                    CreationDate = now,
                    UserId = sla.AssigneeId,
                });

                // Publish event để SlaViolationConsumer xử lý (thu hồi & giao lại)
                await publishEndpoint.Publish(new SlaViolationEvent
                {
                    CustomerId = sla.CustomerId,
                    CustomerName = sla.Customer?.Name ?? "N/A",
                    ViolatedAssigneeId = sla.AssigneeId,
                    ViolatedAssigneeName = sla.Assignee?.FullName ?? "N/A",
                    SlaTrackingId = sla.Id,
                    AssignedAt = sla.AssignedAt,
                    Deadline = sla.Deadline,
                    ViolatedAt = now,
                }, cancellationToken);

                _logger.LogWarning(
                    "SLA Violation: NV {AssigneeName} ({AssigneeId}) không liên hệ KH {CustomerName} ({CustomerId}). " +
                    "Giao lúc: {AssignedAt}, Deadline: {Deadline}",
                    sla.Assignee?.FullName, sla.AssigneeId, sla.Customer?.Name, sla.CustomerId,
                    sla.AssignedAt, sla.Deadline);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Kiểm tra SLA sắp hết hạn (còn 5 phút) — gửi cảnh báo cho NV
        /// </summary>
        private async Task CheckSlaWarningsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var now = DateTime.UtcNow;
            var warningThreshold = now.AddMinutes(WARNING_THRESHOLD_MINUTES);

            // Tìm SLA sắp hết hạn (trong vòng 5 phút nữa) mà chưa liên hệ
            var warnings = await context.SlaTrackings
                .Include(s => s.Customer)
                .Where(s => !s.IsContactMade &&
                            !s.IsViolated &&
                            !s.IsReassigned &&
                            s.Deadline > now &&
                            s.Deadline <= warningThreshold)
                .ToListAsync(cancellationToken);

            foreach (var sla in warnings)
            {
                // Kiểm tra đã gửi cảnh báo chưa (tránh gửi lại mỗi phút)
                var alreadyWarned = await context.Notifications
                    .AnyAsync(n => n.RecipientId == sla.AssigneeId &&
                                   n.ReferenceId == sla.Id &&
                                   n.Type == NotificationType.SlaWarning,
                              cancellationToken);

                if (!alreadyWarned)
                {
                    await notificationService.NotifySlaWarningAsync(
                        sla.AssigneeId, sla.CustomerId, sla.Customer?.Name ?? "N/A", sla.Deadline, cancellationToken);

                    _logger.LogInformation(
                        "SLA Warning sent: NV {AssigneeId} còn {Minutes} phút để liên hệ KH {CustomerName}",
                        sla.AssigneeId, (sla.Deadline - now).TotalMinutes, sla.Customer?.Name);
                }
            }
        }
    }
}
