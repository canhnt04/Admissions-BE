using Crm.Application.Common.Interfaces;
using Crm.Application.Events;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Services
{
    /// <summary>
    /// Implementation cơ chế giao khách tự động theo Round-Robin queue.
    /// SLA timeout = 30 phút.
    /// </summary>
    public class AssignmentService : IAssignmentService
    {
        private readonly CrmDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AssignmentService> _logger;

        /// <summary>
        /// Thời hạn SLA tính bằng phút — NV phải liên hệ KH trong khoảng thời gian này
        /// </summary>
        private const int SLA_DEADLINE_MINUTES = 30;

        public AssignmentService(
            CrmDbContext context,
            IPublishEndpoint publishEndpoint,
            INotificationService notificationService,
            ILogger<AssignmentService> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Guid?> AutoAssignAsync(Guid customerId, TrainingSystem trainingSystem, CancellationToken cancellationToken = default)
        {
            // 1. Tìm NV tiếp theo trong queue (Round-Robin)
            //    Ưu tiên NV có LastAssignedAt cũ nhất (hoặc null = chưa từng nhận lead)
            var nextConsultant = await _context.AssignmentQueues
                .Where(q => q.TrainingSystem == trainingSystem && q.IsActive && q.CurrentLoad < q.MaxLoad)
                .OrderBy(q => q.LastAssignedAt ?? DateTime.MinValue)
                .ThenBy(q => q.OrderIndex)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextConsultant == null)
            {
                _logger.LogWarning(
                    "Không tìm được NV nào trong queue cho nhánh {TrainingSystem}. KH {CustomerId} chưa được giao.",
                    trainingSystem, customerId);
                return null;
            }

            // 2. Lấy thông tin Customer + User
            var customer = await _context.Customers.FindAsync(new object[] { customerId }, cancellationToken);
            if (customer == null) return null;

            var consultant = await _context.Users.FindAsync(new object[] { nextConsultant.ConsultantId }, cancellationToken);
            if (consultant == null) return null;

            var now = DateTime.UtcNow;
            var deadline = now.AddMinutes(SLA_DEADLINE_MINUTES);

            // 3. Giao lead
            customer.Assignee = nextConsultant.ConsultantId;
            customer.UpdateTime = now;

            // 4. Ghi lịch sử giao lead
            _context.CustomerAssignmentHistories.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AssigneeId = nextConsultant.ConsultantId,
                AssignedById = customer.CreatedBy, // System/Creator
                AssignmentDate = now,
                Reason = AssignmentReason.NewLead,
                Note = $"Tự động giao lead (Round-Robin) cho {consultant.FullName}",
            });

            // 5. Tạo SLA tracking (30 phút)
            _context.SlaTrackings.Add(new SlaTracking
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AssigneeId = nextConsultant.ConsultantId,
                AssignedAt = now,
                Deadline = deadline,
                IsContactMade = false,
                IsViolated = false,
                IsReassigned = false,
            });

            // 6. Update queue
            nextConsultant.CurrentLoad += 1;
            nextConsultant.LastAssignedAt = now;

            // 7. Ghi AuditLog
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = Domain.Entities.Action.Assign,
                Detail = $"Auto-assign KH [{customer.Name}] cho NV [{consultant.FullName}]. SLA deadline: {deadline:HH:mm:ss}",
                RecordId = customerId,
                RecordDesc = customer.Name,
                RecordEntity = RecordEntity.Assignment,
                CreationDate = now,
                UserId = customer.CreatedBy,
            });

            await _context.SaveChangesAsync(cancellationToken);

            // 8. Publish event
            await _publishEndpoint.Publish(new LeadAssignedEvent
            {
                CustomerId = customerId,
                CustomerName = customer.Name,
                AssigneeId = nextConsultant.ConsultantId,
                AssigneeName = consultant.FullName,
                AssignedById = customer.CreatedBy,
                Reason = AssignmentReason.NewLead,
                AssignedAt = now,
                SlaDeadline = deadline,
            }, cancellationToken);

            // 9. Gửi notification cho NV
            await _notificationService.NotifyLeadAssignedAsync(
                nextConsultant.ConsultantId, customerId, customer.Name, cancellationToken);

            _logger.LogInformation(
                "Auto-assigned KH {CustomerName} ({CustomerId}) cho NV {ConsultantName}. Deadline: {Deadline}",
                customer.Name, customerId, consultant.FullName, deadline);

            return nextConsultant.ConsultantId;
        }

        /// <inheritdoc/>
        public async Task ManualAssignAsync(Guid customerId, Guid assigneeId, Guid assignedById, string? note = null, CancellationToken cancellationToken = default)
        {
            var customer = await _context.Customers.FindAsync(new object[] { customerId }, cancellationToken)
                ?? throw new InvalidOperationException($"Không tìm thấy khách hàng {customerId}");

            var assignee = await _context.Users.FindAsync(new object[] { assigneeId }, cancellationToken)
                ?? throw new InvalidOperationException($"Không tìm thấy nhân viên {assigneeId}");

            var now = DateTime.UtcNow;
            var deadline = now.AddMinutes(SLA_DEADLINE_MINUTES);

            // Nếu KH đang được giao cho NV khác → giảm load NV cũ
            if (customer.Assignee.HasValue && customer.Assignee != assigneeId)
            {
                var oldQueue = await _context.AssignmentQueues
                    .FirstOrDefaultAsync(q => q.ConsultantId == customer.Assignee.Value &&
                                              q.TrainingSystem == customer.TrainingSystem,
                                         cancellationToken);
                if (oldQueue != null && oldQueue.CurrentLoad > 0)
                {
                    oldQueue.CurrentLoad -= 1;
                }

                // Đánh dấu SLA cũ là đã reassign
                var oldSla = await _context.SlaTrackings
                    .FirstOrDefaultAsync(s => s.CustomerId == customerId &&
                                              s.AssigneeId == customer.Assignee.Value &&
                                              !s.IsReassigned,
                                         cancellationToken);
                if (oldSla != null)
                {
                    oldSla.IsReassigned = true;
                    oldSla.ReassignedAt = now;
                    oldSla.ReassignedToId = assigneeId;
                }
            }

            // Giao lead
            customer.Assignee = assigneeId;
            customer.UpdateTime = now;

            // Ghi lịch sử
            _context.CustomerAssignmentHistories.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AssigneeId = assigneeId,
                AssignedById = assignedById,
                AssignmentDate = now,
                Reason = AssignmentReason.ManualAssign,
                Note = note ?? $"Giao thủ công bởi admin cho {assignee.FullName}",
            });

            // Tạo SLA tracking mới
            _context.SlaTrackings.Add(new SlaTracking
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AssigneeId = assigneeId,
                AssignedAt = now,
                Deadline = deadline,
                IsContactMade = false,
                IsViolated = false,
                IsReassigned = false,
            });

            // Update queue load cho NV mới
            var newQueue = await _context.AssignmentQueues
                .FirstOrDefaultAsync(q => q.ConsultantId == assigneeId &&
                                          q.TrainingSystem == customer.TrainingSystem,
                                     cancellationToken);
            if (newQueue != null)
            {
                newQueue.CurrentLoad += 1;
                newQueue.LastAssignedAt = now;
            }

            // AuditLog
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = Domain.Entities.Action.Assign,
                Detail = $"Manual-assign KH [{customer.Name}] cho NV [{assignee.FullName}] bởi user {assignedById}. Ghi chú: {note}",
                RecordId = customerId,
                RecordDesc = customer.Name,
                RecordEntity = RecordEntity.Assignment,
                CreationDate = now,
                UserId = assignedById,
            });

            await _context.SaveChangesAsync(cancellationToken);

            // Notification
            await _notificationService.NotifyLeadAssignedAsync(
                assigneeId, customerId, customer.Name, cancellationToken);

            _logger.LogInformation(
                "Manual-assigned KH {CustomerName} ({CustomerId}) cho NV {AssigneeName} bởi {AssignedById}",
                customer.Name, customerId, assignee.FullName, assignedById);
        }

        /// <inheritdoc/>
        public async Task<Guid?> ReassignAfterSlaViolationAsync(Guid customerId, Guid violatedAssigneeId, CancellationToken cancellationToken = default)
        {
            var customer = await _context.Customers.FindAsync(new object[] { customerId }, cancellationToken);
            if (customer == null || customer.TrainingSystem == null) return null;

            var now = DateTime.UtcNow;

            // Giảm load NV vi phạm
            var violatedQueue = await _context.AssignmentQueues
                .FirstOrDefaultAsync(q => q.ConsultantId == violatedAssigneeId &&
                                          q.TrainingSystem == customer.TrainingSystem,
                                     cancellationToken);
            if (violatedQueue != null && violatedQueue.CurrentLoad > 0)
            {
                violatedQueue.CurrentLoad -= 1;
            }

            // Tìm NV tiếp theo trong queue (skip NV vi phạm)
            var nextConsultant = await _context.AssignmentQueues
                .Where(q => q.TrainingSystem == customer.TrainingSystem &&
                            q.IsActive &&
                            q.CurrentLoad < q.MaxLoad &&
                            q.ConsultantId != violatedAssigneeId) // Skip NV bị vi phạm
                .OrderBy(q => q.LastAssignedAt ?? DateTime.MinValue)
                .ThenBy(q => q.OrderIndex)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextConsultant == null)
            {
                _logger.LogWarning(
                    "Không tìm được NV thay thế cho KH {CustomerId} sau SLA violation. KH chưa được giao lại.",
                    customerId);
                return null;
            }

            var newConsultant = await _context.Users.FindAsync(new object[] { nextConsultant.ConsultantId }, cancellationToken);
            if (newConsultant == null) return null;

            var deadline = now.AddMinutes(SLA_DEADLINE_MINUTES);

            // Giao lại lead
            customer.Assignee = nextConsultant.ConsultantId;
            customer.UpdateTime = now;

            // Ghi lịch sử
            _context.CustomerAssignmentHistories.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AssigneeId = nextConsultant.ConsultantId,
                AssignedById = violatedAssigneeId, // Ghi lại NV cũ bị vi phạm
                AssignmentDate = now,
                Reason = AssignmentReason.SlaViolation,
                Note = $"Thu hồi từ NV vi phạm SLA, giao lại cho {newConsultant.FullName}",
            });

            // Tạo SLA tracking mới cho NV mới
            _context.SlaTrackings.Add(new SlaTracking
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
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

            // AuditLog
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = Domain.Entities.Action.AutoReassign,
                Detail = $"SLA Violation: Thu hồi KH [{customer.Name}] từ NV {violatedAssigneeId}, giao lại cho NV [{newConsultant.FullName}]",
                RecordId = customerId,
                RecordDesc = customer.Name,
                RecordEntity = RecordEntity.Assignment,
                CreationDate = now,
                UserId = violatedAssigneeId,
            });

            await _context.SaveChangesAsync(cancellationToken);

            // Notification cho NV mới
            await _notificationService.NotifyLeadReassignedAsync(
                nextConsultant.ConsultantId, customerId, customer.Name, "SLA Violation — lead giao lại", cancellationToken);

            // Notification cho NV bị vi phạm
            await _notificationService.NotifySlaViolationAsync(
                violatedAssigneeId, customerId, customer.Name, cancellationToken);

            _logger.LogInformation(
                "SLA Reassigned: KH {CustomerName} ({CustomerId}) từ NV {OldAssignee} sang NV {NewAssignee}",
                customer.Name, customerId, violatedAssigneeId, newConsultant.FullName);

            return nextConsultant.ConsultantId;
        }
    }
}
