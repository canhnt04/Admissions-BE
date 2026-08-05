using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Application.Events;
using LeadAssignment.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeadAssignment.Infrastructure.Services
{
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
            var customerCareStatusRepository = scope.ServiceProvider.GetRequiredService<ICustomerCareStatusRepository>();
            var auditLogRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
            var context = scope.ServiceProvider.GetRequiredService<IAssignmentDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var userGrpcClient = scope.ServiceProvider.GetRequiredService<IUserGrpcClient>();
            var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
            var slaSettings = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LeadAssignment.Application.Common.Models.SlaSettings>>().Value;
            var now = DateTime.UtcNow;

            var managerIds = slaSettings.Managers.Values.ToList();
            if (slaSettings.DefaultManagerId != Guid.Empty) managerIds.Add(slaSettings.DefaultManagerId);

            var pendingLeads = await customerCareStatusRepository.Query()
                .Where(s => s.Status == LeadStatus.New && s.AssigneeId != null)
                .ToListAsync(cancellationToken);

            if (pendingLeads.Count == 0) return;

            var violations = new System.Collections.Generic.List<CustomerCareStatus>();
            foreach (var sla in pendingLeads)
            {
                var assigneeId = sla.AssigneeId!.Value;
                bool isManager = managerIds.Contains(assigneeId);
                var baseSlaMins = isManager ? slaSettings.AdminSlaDeadlineMinutes : slaSettings.SlaDeadlineMinutes;
                
                int currentLoad = await customerCareStatusRepository.Query()
                    .CountAsync(c => c.AssigneeId == assigneeId && c.Status == LeadStatus.New && c.TrainingSystem == sla.TrainingSystem, cancellationToken);
                    
                int multiplier = Math.Min(slaSettings.MaxSlaMultiplier, Math.Max(1, currentLoad));
                
                var deadline = (sla.StatusDate ?? now).AddMinutes(baseSlaMins * multiplier);
                
                if (now >= deadline)
                {
                    violations.Add(sla);
                }
            }

            if (violations.Count == 0) return;

            _logger.LogWarning("Phát hiện {Count} SLA violations", violations.Count);

            var assigneeIds = violations.Select(v => v.AssigneeId!.Value).Distinct().ToList();
            var fullNames = await userGrpcClient.GetUserFullNamesAsync(assigneeIds, cancellationToken);

            foreach (var sla in violations)
            {
                var assigneeId = sla.AssigneeId!.Value;
                var assigneeName = fullNames[assigneeId];
                var assignedAt = sla.StatusDate ?? now;

                _logger.LogWarning(
                    "SLA Violation: Nhân viên {AssigneeName} ({AssigneeId}) không liên hệ KH {CustomerName} ({CustomerId}). Giao lúc: {AssignedAt}",
                    assigneeName, assigneeId, sla.CustomerName, sla.CustomerId, assignedAt);

                await mediator.Send(new LeadAssignment.Application.Assignments.Commands.ReassignAfterSlaViolation.ReassignAfterSlaViolationCommand
                {
                    CustomerId = sla.CustomerId,
                    ViolatedAssigneeId = assigneeId
                }, cancellationToken);
            }
        }

        private async Task CheckSlaWarningsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var customerCareStatusRepository = scope.ServiceProvider.GetRequiredService<ICustomerCareStatusRepository>();
            var auditLogRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
            var context = scope.ServiceProvider.GetRequiredService<IAssignmentDbContext>();
            var emailSender = scope.ServiceProvider.GetRequiredService<LeadAssignment.Application.Common.Interfaces.IEmailSender>();
            var slaSettings = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LeadAssignment.Application.Common.Models.SlaSettings>>().Value;
            var now = DateTime.UtcNow;

            var managerIds = slaSettings.Managers.Values.ToList();
            if (slaSettings.DefaultManagerId != Guid.Empty) managerIds.Add(slaSettings.DefaultManagerId);

            var pendingLeads = await customerCareStatusRepository.Query()
                .Where(s => s.Status == LeadStatus.New && s.AssigneeId != null)
                .ToListAsync(cancellationToken);

            if (pendingLeads.Count == 0) return;

            var warnings = new System.Collections.Generic.List<(CustomerCareStatus Sla, DateTime Deadline)>();
            foreach (var sla in pendingLeads)
            {
                var assigneeId = sla.AssigneeId!.Value;
                bool isManager = managerIds.Contains(assigneeId);
                var baseSlaMins = isManager ? slaSettings.AdminSlaDeadlineMinutes : slaSettings.SlaDeadlineMinutes;
                
                int currentLoad = await customerCareStatusRepository.Query()
                    .CountAsync(c => c.AssigneeId == assigneeId && c.Status == LeadStatus.New && c.TrainingSystem == sla.TrainingSystem, cancellationToken);
                    
                int multiplier = Math.Min(slaSettings.MaxSlaMultiplier, Math.Max(1, currentLoad));
                
                var deadline = (sla.StatusDate ?? now).AddMinutes(baseSlaMins * multiplier);
                
                if (now >= deadline.AddMinutes(-5) && now < deadline)
                {
                    warnings.Add((sla, deadline));
                }
            }

            if (warnings.Count == 0) return;

            foreach (var item in warnings)
            {
                var sla = item.Sla;
                var deadline = item.Deadline;
                var assigneeId = sla.AssigneeId!.Value;
                
                var alreadyWarned = await auditLogRepository.Query()
                    .AnyAsync(a => a.RecordId == sla.CustomerId && a.Action == LeadAssignment.Domain.Enums.Action.Update && a.Detail.Contains("SLA_WARNING_SENT"), cancellationToken);
                    
                if (!alreadyWarned)
                {
                    auditLogRepository.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        Action = LeadAssignment.Domain.Enums.Action.Update,
                        Detail = $"[SLA_WARNING_SENT] Cảnh báo SLA cho KH {sla.CustomerName}",
                        RecordId = sla.CustomerId,
                        RecordDesc = sla.CustomerName,
                        RecordEntity = RecordEntity.Customer,
                        CreationDate = now,
                        UserId = Guid.Empty
                    });
                    await context.SaveChangesAsync(cancellationToken);
                    
                    await emailSender.SendEmailAsync(
                        $"{assigneeId}@system.local",
                        $"[Cảnh báo] Bạn có 1 Lead chưa xử lý sắp hết hạn",
                        $"<p>Khách hàng {sla.CustomerName} sắp hết hạn SLA vào lúc {deadline:HH:mm:ss dd/MM/yyyy}. Vui lòng xử lý ngay lập tức.</p>",
                        cancellationToken);
                        
                    _logger.LogInformation("SLA Warning sent: NV {AssigneeId} còn {Minutes} phút để liên hệ KH {CustomerName}", assigneeId, (deadline - now).TotalMinutes, sla.CustomerName);
                }
            }
        }
    }
}
