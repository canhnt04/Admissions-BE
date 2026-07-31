using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Application.Events;
using LeadAssignment.Infrastructure.Data;
using LeadAssignment.Domain.Entities;
using LeadAssignment.Application.Events;
using LeadAssignment.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadAssignment.Infrastructure.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly AssignmentDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AssignmentService> _logger;

        private const int DEFAULT_SLA_DEADLINE_MINUTES = 30;

        public AssignmentService(
            AssignmentDbContext context,
            IPublishEndpoint publishEndpoint,
            INotificationService notificationService,
            IEmailSender emailSender,
            ILogger<AssignmentService> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _notificationService = notificationService;
            _emailSender = emailSender;
            _logger = logger;
        }

        private async Task<int> GetSlaDeadlineMinutesAsync(CancellationToken cancellationToken)
        {
            var config = await _context.SystemConfigs.FindAsync(new object[] { "SlaDeadlineMinutes" }, cancellationToken);
            if (config != null && int.TryParse(config.Value, out var mins)) return mins;
            return DEFAULT_SLA_DEADLINE_MINUTES;
        }

        private async Task<Guid?> GetDefaultManagerIdAsync(CancellationToken cancellationToken)
        {
            var config = await _context.SystemConfigs.FindAsync(new object[] { "DefaultManagerId" }, cancellationToken);
            if (config != null && Guid.TryParse(config.Value, out var managerId)) return managerId;
            return null;
        }

        public async Task<Guid?> AutoAssignAsync(Guid customerId, TrainingSystem trainingSystem, CancellationToken cancellationToken = default)
        {
            // Note: In AutoAssign (called from Consumer), we receive CustomerName via the event.
            // But since AutoAssignAsync signature here doesn't have CustomerName, we should fetch it or change signature.
            // Let's modify the signature to include CustomerName or rely on a wrapper. 
            // Wait, I updated the interface to Task<Guid?> AutoAssignAsync(Guid customerId, TrainingSystem trainingSystem, CancellationToken cancellationToken = default) 
            // I should just use a default name if not found. Or I can change the interface.
            // Let's just find the customerName from history or use "N/A".
            var nameRecord = await _context.CustomerCareStatuses
                .Where(x => x.CustomerId == customerId)
                .Select(x => x.CustomerName)
                .FirstOrDefaultAsync(cancellationToken);
            
            var customerName = nameRecord ?? "New Customer";

            var nextConsultant = await _context.AssignmentQueues
                .Where(q => q.TrainingSystem == trainingSystem && q.IsActive && q.CurrentLoad < q.MaxLoad)
                .OrderBy(q => q.LastAssignedAt ?? DateTime.MinValue)
                .ThenBy(q => q.OrderIndex)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextConsultant == null)
            {
                _logger.LogWarning("Không tìm được NV nào trong queue cho nhánh {TrainingSystem}. KH {CustomerId} chưa được giao.", trainingSystem, customerId);
                return null;
            }

            var consultantReplica = await _context.UserReplicas.FindAsync(new object[] { nextConsultant.ConsultantId }, cancellationToken);
            var consultantName = consultantReplica?.FullName ?? "N/A";

            var now = DateTime.UtcNow;
            var slaMinutes = await GetSlaDeadlineMinutesAsync(cancellationToken);
            var deadline = now.AddMinutes(slaMinutes);

            // Ghi lịch sử giao lead
            _context.CustomerAssignmentHistories.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AssigneeId = nextConsultant.ConsultantId,
                AssignedById = Guid.Empty, // System
                AssignmentDate = now,
                Reason = AssignmentReason.NewLead,
                Note = $"Tự động giao lead (Round-Robin) cho {consultantName}",
            });

            // Tạo SLA tracking
            _context.CustomerCareStatuses.Add(new CustomerCareStatus
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerName = customerName,
                TrainingSystem = trainingSystem,
                AssigneeId = nextConsultant.ConsultantId,
                AssignedAt = now,
                Deadline = deadline,
                IsContactMade = false,
                IsViolated = false,
                IsReassigned = false,
            });

            // Update queue
            nextConsultant.CurrentLoad += 1;
            nextConsultant.LastAssignedAt = now;

            // Ghi AuditLog
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Entities.Action.Assign,
                Detail = $"Auto-assign KH [{customerName}] cho NV [{consultantName}]. SLA deadline: {deadline:HH:mm:ss}",
                RecordId = customerId,
                RecordDesc = customerName,
                RecordEntity = RecordEntity.Assignment,
                CreationDate = now,
                UserId = Guid.Empty,
            });

            await _context.SaveChangesAsync(cancellationToken);

            // Publish event
            await _publishEndpoint.Publish(new LeadAssignedEvent
            {
                CustomerId = customerId,
                CustomerName = customerName,
                AssigneeId = nextConsultant.ConsultantId,
                AssigneeName = consultantName,
                AssignedById = Guid.Empty,
                Reason = AssignmentReason.NewLead,
                AssignedAt = now,
                SlaDeadline = deadline,
            }, cancellationToken);

            await _notificationService.NotifyLeadAssignedAsync(
                nextConsultant.ConsultantId, customerId, customerName, cancellationToken);
            
            await _emailSender.SendEmailAsync(
                $"{nextConsultant.ConsultantId}@system.local",
                "Bạn được giao khách hàng mới",
                $"<p>Chào bạn, bạn vừa được tự động phân bổ một khách hàng mới: {customerName}. Vui lòng liên hệ và chốt sales trước {deadline:HH:mm}!</p>",
                cancellationToken);

            _logger.LogInformation("Auto-assigned KH {CustomerName} ({CustomerId}) cho NV {ConsultantName}. Deadline: {Deadline}", customerName, customerId, consultantName, deadline);

            return nextConsultant.ConsultantId;
        }

        public async Task ManualAssignAsync(Guid customerId, Guid assigneeId, Guid assignedById, string? note = null, CancellationToken cancellationToken = default)
        {
            var latestStatus = await _context.CustomerCareStatuses
                .Where(s => s.CustomerId == customerId && !s.IsReassigned)
                .OrderByDescending(s => s.AssignedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var customerName = latestStatus?.CustomerName ?? "Unknown Customer";
            var trainingSystem = latestStatus?.TrainingSystem ?? TrainingSystem.ShortTerm;

            var assigneeReplica = await _context.UserReplicas.FindAsync(new object[] { assigneeId }, cancellationToken);
            var assigneeName = assigneeReplica?.FullName ?? "N/A";

            var now = DateTime.UtcNow;
            var slaMinutes = await GetSlaDeadlineMinutesAsync(cancellationToken);
            var deadline = now.AddMinutes(slaMinutes);

            if (latestStatus != null && latestStatus.AssigneeId != assigneeId)
            {
                var oldQueue = await _context.AssignmentQueues
                    .FirstOrDefaultAsync(q => q.ConsultantId == latestStatus.AssigneeId &&
                                              q.TrainingSystem == trainingSystem,
                                         cancellationToken);
                if (oldQueue != null && oldQueue.CurrentLoad > 0)
                {
                    oldQueue.CurrentLoad -= 1;
                }

                latestStatus.IsReassigned = true;
                latestStatus.ReassignedAt = now;
                latestStatus.ReassignedToId = assigneeId;
            }

            _context.CustomerAssignmentHistories.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AssigneeId = assigneeId,
                AssignedById = assignedById,
                AssignmentDate = now,
                Reason = AssignmentReason.ManualAssign,
                Note = note ?? $"Giao thủ công bởi admin cho {assigneeName}",
            });

            _context.CustomerCareStatuses.Add(new CustomerCareStatus
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerName = customerName,
                TrainingSystem = trainingSystem,
                AssigneeId = assigneeId,
                AssignedAt = now,
                Deadline = deadline,
                IsContactMade = false,
                IsViolated = false,
                IsReassigned = false,
            });

            var newQueue = await _context.AssignmentQueues
                .FirstOrDefaultAsync(q => q.ConsultantId == assigneeId &&
                                          q.TrainingSystem == trainingSystem,
                                     cancellationToken);
            if (newQueue != null)
            {
                newQueue.CurrentLoad += 1;
                newQueue.LastAssignedAt = now;
            }

            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Entities.Action.Assign,
                Detail = $"Manual-assign KH [{customerName}] cho NV [{assigneeName}] bởi user {assignedById}. Ghi chú: {note}",
                RecordId = customerId,
                RecordDesc = customerName,
                RecordEntity = RecordEntity.Assignment,
                CreationDate = now,
                UserId = assignedById,
            });

            await _context.SaveChangesAsync(cancellationToken);

            // Publish Event
            await _publishEndpoint.Publish(new LeadAssignedEvent
            {
                CustomerId = customerId,
                CustomerName = customerName,
                AssigneeId = assigneeId,
                AssigneeName = assigneeName,
                AssignedById = assignedById,
                Reason = AssignmentReason.ManualAssign,
                AssignedAt = now,
                SlaDeadline = deadline,
            }, cancellationToken);

            await _notificationService.NotifyLeadAssignedAsync(assigneeId, customerId, customerName, cancellationToken);
            await _emailSender.SendEmailAsync(
                $"{assigneeId}@system.local",
                "Bạn được giao khách hàng thủ công",
                $"<p>Chào bạn, bạn được phân bổ thủ công một khách hàng mới: {customerName}. Vui lòng liên hệ và chốt sales trước {deadline:HH:mm}!</p>",
                cancellationToken);
        }

        public async Task<Guid?> ReassignAfterSlaViolationAsync(Guid customerId, Guid violatedAssigneeId, CancellationToken cancellationToken = default)
        {
            var latestStatus = await _context.CustomerCareStatuses
                .Where(s => s.CustomerId == customerId && s.AssigneeId == violatedAssigneeId)
                .OrderByDescending(s => s.AssignedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestStatus == null) return null;

            var customerName = latestStatus.CustomerName;
            var trainingSystem = latestStatus.TrainingSystem;
            var now = DateTime.UtcNow;

            var violatedQueue = await _context.AssignmentQueues
                .FirstOrDefaultAsync(q => q.ConsultantId == violatedAssigneeId &&
                                          q.TrainingSystem == trainingSystem,
                                     cancellationToken);
            if (violatedQueue != null && violatedQueue.CurrentLoad > 0)
            {
                violatedQueue.CurrentLoad -= 1;
            }

            // Đếm số lần vi phạm SLA cho customer này
            var reassignmentCount = await _context.CustomerCareStatuses
                .CountAsync(s => s.CustomerId == customerId && s.IsViolated, cancellationToken);
                
            var isThreeStrikes = reassignmentCount >= 3;
            
            Guid? nextAssigneeId = null;
            string newConsultantName = "N/A";
            
            if (isThreeStrikes)
            {
                var managerId = await GetDefaultManagerIdAsync(cancellationToken);
                if (managerId.HasValue)
                {
                    nextAssigneeId = managerId.Value;
                    var managerReplica = await _context.UserReplicas.FindAsync(new object[] { managerId.Value }, cancellationToken);
                    newConsultantName = managerReplica?.FullName ?? "Manager";
                    
                    await _emailSender.SendEmailAsync(
                        $"{managerId.Value}@system.local",
                        "CẢNH BÁO ESCALATION: Khách hàng vi phạm SLA 3 lần",
                        $"<p>Khách hàng {customerName} đã vi phạm SLA 3 lần liên tiếp do các nhân viên không liên hệ. Hệ thống đã thu hồi và giao lại cho bạn (Quản lý) xử lý.</p>",
                        cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Khách hàng {CustomerId} vi phạm SLA 3 lần nhưng chưa cấu hình DefaultManagerId. Sẽ tiếp tục vòng lặp Round-Robin.", customerId);
                }
            }
            
            if (nextAssigneeId == null)
            {
                var nextConsultant = await _context.AssignmentQueues
                    .Where(q => q.TrainingSystem == trainingSystem &&
                                q.IsActive &&
                                q.CurrentLoad < q.MaxLoad &&
                                q.ConsultantId != violatedAssigneeId)
                    .OrderBy(q => q.LastAssignedAt ?? DateTime.MinValue)
                    .ThenBy(q => q.OrderIndex)
                    .FirstOrDefaultAsync(cancellationToken);
    
                if (nextConsultant == null)
                {
                    _logger.LogWarning("Không tìm được NV thay thế cho KH {CustomerId} sau SLA violation. KH chưa được giao lại.", customerId);
                    return null;
                }
                
                nextAssigneeId = nextConsultant.ConsultantId;
                var newConsultantReplica = await _context.UserReplicas.FindAsync(new object[] { nextAssigneeId }, cancellationToken);
                newConsultantName = newConsultantReplica?.FullName ?? "N/A";
                
                nextConsultant.CurrentLoad += 1;
                nextConsultant.LastAssignedAt = now;
            }

            var slaMinutes = await GetSlaDeadlineMinutesAsync(cancellationToken);
            var deadline = now.AddMinutes(slaMinutes);

            latestStatus.IsReassigned = true;
            latestStatus.ReassignedAt = now;
            latestStatus.ReassignedToId = nextAssigneeId.Value;

            _context.CustomerAssignmentHistories.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AssigneeId = nextAssigneeId.Value,
                AssignedById = violatedAssigneeId,
                AssignmentDate = now,
                Reason = AssignmentReason.SlaViolation,
                Note = isThreeStrikes ? $"Vi phạm 3 lần -> Bắn lên Manager: {newConsultantName}" : $"Thu hồi từ NV vi phạm SLA, giao lại cho {newConsultantName}",
            });

            _context.CustomerCareStatuses.Add(new CustomerCareStatus
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerName = customerName,
                TrainingSystem = trainingSystem,
                AssigneeId = nextAssigneeId.Value,
                AssignedAt = now,
                Deadline = deadline,
                IsContactMade = false,
                IsViolated = false,
                IsReassigned = false,
            });

            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Entities.Action.AutoReassign,
                Detail = $"SLA Violation: Thu hồi KH [{customerName}] từ NV {violatedAssigneeId}, giao lại cho [{newConsultantName}]",
                RecordId = customerId,
                RecordDesc = customerName,
                RecordEntity = RecordEntity.Assignment,
                CreationDate = now,
                UserId = violatedAssigneeId,
            });

            await _context.SaveChangesAsync(cancellationToken);

            // Publish Event
            await _publishEndpoint.Publish(new LeadAssignedEvent
            {
                CustomerId = customerId,
                CustomerName = customerName,
                AssigneeId = nextAssigneeId.Value,
                AssigneeName = newConsultantName,
                AssignedById = violatedAssigneeId,
                Reason = AssignmentReason.SlaViolation,
                AssignedAt = now,
                SlaDeadline = deadline,
            }, cancellationToken);

            await _notificationService.NotifyLeadReassignedAsync(
                nextAssigneeId.Value, customerId, customerName, isThreeStrikes ? "Lead vi phạm SLA 3 lần" : "SLA Violation — lead giao lại", cancellationToken);

            await _notificationService.NotifySlaViolationAsync(violatedAssigneeId, customerId, customerName, cancellationToken);
            await _emailSender.SendEmailAsync(
                $"{violatedAssigneeId}@system.local",
                "CẢNH BÁO: Vi phạm SLA Khách hàng",
                $"<p>Bạn đã vi phạm SLA khi không liên hệ khách hàng {customerName} trong thời gian quy định. Lead này đã bị hệ thống thu hồi tự động.</p>",
                cancellationToken);

            _logger.LogInformation("SLA Reassigned: KH {CustomerName} ({CustomerId}) từ NV {OldAssignee} sang {NewAssignee}", customerName, customerId, violatedAssigneeId, newConsultantName);

            return nextAssigneeId.Value;
        }
    }
}


