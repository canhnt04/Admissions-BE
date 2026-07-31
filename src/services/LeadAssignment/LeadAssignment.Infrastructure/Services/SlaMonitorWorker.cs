using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Application.Events;
using LeadAssignment.Infrastructure.Data;
using LeadAssignment.Domain.Entities;




using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LeadAssignment.Infrastructure.Services
{
    /// <summary>
    /// Background service kiểm tra SLA mỗi phút.
    /// SLA timeout = 30 phút. Nếu NV không liên hệ KH trong 30 phút → vi phạm SLA → thu hồi lead.
    /// 
    /// Sử dụng AssignmentDbContext — lookup tên NV qua bảng UserReplica nội bộ.
    /// </summary>
    public class SlaMonitorWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SlaMonitorWorker> _logger;

        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);
        private const int WARNING_THRESHOLD_MINUTES = 5;

        public SlaMonitorWorker(IServiceScopeFactory scopeFactory, ILogger<SlaMonitorWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SlaMonitorWorker started. Check interval: {Interval}", CheckInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckSlaViolationsAsync(stoppingToken);
                    await CheckSlaWarningsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong SlaMonitorWorker");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }

            _logger.LogInformation("SlaMonitorWorker stopped.");
        }

        private async Task CheckSlaViolationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AssignmentDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var now = DateTime.UtcNow;

            // Tìm tất cả SLA đã quá hạn mà chưa xử lý
            var violations = await context.CustomerCareStatuses
                
                .Where(s => !s.IsContactMade && s.Deadline < now && !s.IsReassigned && !s.IsViolated)
                .ToListAsync(cancellationToken);

            if (violations.Count == 0) return;

            _logger.LogWarning("Phát hiện {Count} SLA violations", violations.Count);

            foreach (var sla in violations)
            {
                // Lookup tên NV từ UserReplica
                var assigneeReplica = await context.UserReplicas.FindAsync(new object[] { sla.AssigneeId }, cancellationToken);
                var assigneeName = assigneeReplica?.FullName ?? "N/A";

                // Mark violated
                sla.IsViolated = true;

                // Ghi AuditLog
                context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = Domain.Entities.Action.SlaViolation,
                    Detail = $"SLA Violation: NV [{assigneeName}] không liên hệ KH [{sla.CustomerName}] trong 30 phút. " +
                             $"Giao lúc: {sla.AssignedAt:HH:mm:ss}, Deadline: {sla.Deadline:HH:mm:ss}",
                    RecordId = sla.CustomerId,
                    RecordDesc = sla.CustomerName,
                    RecordEntity = RecordEntity.SlaTracking,
                    CreationDate = now,
                    UserId = sla.AssigneeId,
                });

                // Publish event
                await publishEndpoint.Publish(new SlaViolationEvent
                {
                    CustomerId = sla.CustomerId,
                    CustomerName = sla.CustomerName,
                    ViolatedAssigneeId = sla.AssigneeId,
                    ViolatedAssigneeName = assigneeName,
                    SlaTrackingId = sla.Id,
                    AssignedAt = sla.AssignedAt,
                    Deadline = sla.Deadline,
                    ViolatedAt = now,
                }, cancellationToken);

                _logger.LogWarning(
                    "SLA Violation: NV {AssigneeName} ({AssigneeId}) không liên hệ KH {CustomerName} ({CustomerId}). " +
                    "Giao lúc: {AssignedAt}, Deadline: {Deadline}",
                    assigneeName, sla.AssigneeId, sla.CustomerName, sla.CustomerId,
                    sla.AssignedAt, sla.Deadline);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task CheckSlaWarningsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AssignmentDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var now = DateTime.UtcNow;
            var warningThreshold = now.AddMinutes(WARNING_THRESHOLD_MINUTES);

            var warnings = await context.CustomerCareStatuses
                
                .Where(s => !s.IsContactMade &&
                            !s.IsViolated &&
                            !s.IsReassigned &&
                            s.Deadline > now &&
                            s.Deadline <= warningThreshold)
                .ToListAsync(cancellationToken);

            foreach (var sla in warnings)
            {
                var alreadyWarned = await context.Notifications
                    .AnyAsync(n => n.RecipientId == sla.AssigneeId &&
                                   n.ReferenceId == sla.Id &&
                                   n.Type == NotificationType.SlaWarning,
                              cancellationToken);

                if (!alreadyWarned)
                {
                    await notificationService.NotifySlaWarningAsync(
                        sla.AssigneeId, sla.CustomerId, sla.CustomerName, sla.Deadline, cancellationToken);

                    _logger.LogInformation(
                        "SLA Warning sent: NV {AssigneeId} còn {Minutes} phút để liên hệ KH {CustomerName}",
                        sla.AssigneeId, (sla.Deadline - now).TotalMinutes, sla.CustomerName);
                }
            }
        }
    }
}



