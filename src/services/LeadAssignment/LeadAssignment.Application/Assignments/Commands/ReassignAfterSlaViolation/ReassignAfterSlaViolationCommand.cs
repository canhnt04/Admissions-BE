using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Application.Events;
using LeadAssignment.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Commands.ReassignAfterSlaViolation
{
    public class ReassignAfterSlaViolationCommand : IRequest<Result<Guid?>>
    {
        public Guid CustomerId { get; set; }
        public Guid ViolatedAssigneeId { get; set; }
    }

    public class ReassignAfterSlaViolationCommandHandler : IRequestHandler<ReassignAfterSlaViolationCommand, Result<Guid?>>
    {
        private readonly ICustomerCareStatusRepository _customerCareStatusRepository;
        private readonly IAssignmentQueueRepository _assignmentQueueRepository;
        private readonly ICustomerAssignmentHistoryRepository _customerAssignmentHistoryRepository;
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAssignmentDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;
        private readonly IUserGrpcClient _userGrpcClient;
        private readonly ILogger<ReassignAfterSlaViolationCommandHandler> _logger;

        private const int DEFAULT_SLA_DEADLINE_MINUTES = 30;

        public ReassignAfterSlaViolationCommandHandler(
            ICustomerCareStatusRepository customerCareStatusRepository,
            IAssignmentQueueRepository assignmentQueueRepository,
            ICustomerAssignmentHistoryRepository customerAssignmentHistoryRepository,
            ISystemConfigRepository systemConfigRepository,
            IAuditLogRepository auditLogRepository,
            IAssignmentDbContext context,
            IPublishEndpoint publishEndpoint,
            INotificationService notificationService,
            IEmailSender emailSender,
            IUserGrpcClient userGrpcClient,
            ILogger<ReassignAfterSlaViolationCommandHandler> logger)
        {
            _customerCareStatusRepository = customerCareStatusRepository;
            _assignmentQueueRepository = assignmentQueueRepository;
            _customerAssignmentHistoryRepository = customerAssignmentHistoryRepository;
            _systemConfigRepository = systemConfigRepository;
            _auditLogRepository = auditLogRepository;
            _context = context;
            _publishEndpoint = publishEndpoint;
            _notificationService = notificationService;
            _emailSender = emailSender;
            _userGrpcClient = userGrpcClient;
            _logger = logger;
        }

        private async Task<int> GetSlaDeadlineMinutesAsync(CancellationToken cancellationToken)
        {
            var config = await _systemConfigRepository.FirstOrDefaultAsync(x => x.Id == "SlaDeadlineMinutes", cancellationToken);
            if (config != null && int.TryParse(config.Value, out var mins)) return mins;
            return DEFAULT_SLA_DEADLINE_MINUTES;
        }

        private async Task<Guid?> GetDefaultManagerIdAsync(CancellationToken cancellationToken)
        {
            var config = await _systemConfigRepository.FirstOrDefaultAsync(x => x.Id == "DefaultManagerId", cancellationToken);
            if (config != null && Guid.TryParse(config.Value, out var managerId)) return managerId;
            return null;
        }

        public async Task<Result<Guid?>> Handle(ReassignAfterSlaViolationCommand request, CancellationToken cancellationToken)
        {
            var latestStatus = await _customerCareStatusRepository.Query()
                .Where(s => s.CustomerId == request.CustomerId && s.AssigneeId == request.ViolatedAssigneeId)
                .OrderByDescending(s => s.AssignedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestStatus == null) return Result<Guid?>.Success(null);

            var customerName = latestStatus.CustomerName;
            var trainingSystem = latestStatus.TrainingSystem;
            var now = DateTime.UtcNow;

            var violatedQueue = await _assignmentQueueRepository.GetByConsultantAndSystemAsync(request.ViolatedAssigneeId, trainingSystem, cancellationToken);
            if (violatedQueue != null && violatedQueue.CurrentLoad > 0)
            {
                violatedQueue.CurrentLoad -= 1;
                _assignmentQueueRepository.Update(violatedQueue);
            }

            // Đếm số lần vi phạm SLA cho customer này
            var reassignmentCount = await _customerCareStatusRepository.CountSlaViolationsAsync(request.CustomerId, cancellationToken);

            var isThreeStrikes = reassignmentCount >= 3;

            Guid? nextAssigneeId = null;

            if (isThreeStrikes)
            {
                var managerId = await GetDefaultManagerIdAsync(cancellationToken);
                if (managerId.HasValue)
                {
                    nextAssigneeId = managerId.Value;
                }
                else
                {
                    _logger.LogWarning(
                        "Khách hàng {CustomerId} vi phạm SLA 3 lần nhưng chưa cấu hình DefaultManagerId. Sẽ tiếp tục vòng lặp Round-Robin.",
                        request.CustomerId);
                }
            }

            if (nextAssigneeId == null)
            {
                var nextConsultant = await _assignmentQueueRepository.GetNextInQueueAsync(trainingSystem, request.ViolatedAssigneeId, cancellationToken);

                if (nextConsultant == null)
                {
                    _logger.LogWarning(
                        "Không tìm được NV thay thế cho KH {CustomerId} sau SLA violation. KH chưa được giao lại.",
                        request.CustomerId);
                    return Result<Guid?>.Success(null);
                }

                nextAssigneeId = nextConsultant.ConsultantId;
                nextConsultant.CurrentLoad += 1;
                nextConsultant.LastAssignedAt = now;
                _assignmentQueueRepository.Update(nextConsultant);
            }

            // Resolve tên NV mới qua gRPC (batch call)
            var resolvedNames = await _userGrpcClient.GetUserNamesAsync(
                new[] { nextAssigneeId.Value }, cancellationToken);
            var newConsultantName = resolvedNames.GetValueOrDefault(nextAssigneeId.Value, "Unknown");

            if (isThreeStrikes)
            {
                await _emailSender.SendEmailAsync(
                    $"{nextAssigneeId.Value}@system.local",
                    "CẢNH BÁO ESCALATION: Khách hàng vi phạm SLA 3 lần",
                    $"<p>Khách hàng {customerName} đã vi phạm SLA 3 lần liên tiếp do các nhân viên không liên hệ. Hệ thống đã thu hồi và giao lại cho bạn ({newConsultantName}) xử lý.</p>",
                    cancellationToken);
            }

            var slaMinutes = await GetSlaDeadlineMinutesAsync(cancellationToken);
            var deadline = now.AddMinutes(slaMinutes);

            latestStatus.IsReassigned = true;
            latestStatus.ReassignedAt = now;
            latestStatus.ReassignedToId = nextAssigneeId.Value;
            _customerCareStatusRepository.Update(latestStatus);

            _customerAssignmentHistoryRepository.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                AssigneeId = nextAssigneeId.Value,
                AssignedById = request.ViolatedAssigneeId,
                AssignmentDate = now,
                Reason = AssignmentReason.SlaViolation,
                Note = isThreeStrikes
                    ? $"Vi phạm 3 lần -> Bắn lên Manager: {newConsultantName}"
                    : $"Thu hồi từ NV vi phạm SLA, giao lại cho {newConsultantName}",
            });

            _customerCareStatusRepository.Add(new CustomerCareStatus
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                CustomerName = customerName,
                TrainingSystem = trainingSystem,
                AssigneeId = nextAssigneeId.Value,
                AssignedAt = now,
                Deadline = deadline,
                IsContactMade = false,
                IsViolated = false,
                IsReassigned = false,
            });

            _auditLogRepository.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Enums.Action.Insert,
                Detail = $"SLA Violation: Thu hồi KH [{customerName}] từ NV {request.ViolatedAssigneeId}, giao lại cho [{newConsultantName}]",
                RecordId = request.CustomerId,
                RecordDesc = customerName,
                RecordEntity = RecordEntity.Customer,
                CreationDate = now,
                UserId = request.ViolatedAssigneeId,
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new LeadAssignedEvent
            {
                CustomerId = request.CustomerId,
                CustomerName = customerName,
                AssigneeId = nextAssigneeId.Value,
                AssigneeName = newConsultantName,
                AssignedById = request.ViolatedAssigneeId,
                Reason = AssignmentReason.SlaViolation,
                AssignedAt = now,
                SlaDeadline = deadline,
            }, cancellationToken);

            await _notificationService.NotifyLeadReassignedAsync(
                nextAssigneeId.Value, request.CustomerId, customerName,
                isThreeStrikes ? "Lead vi phạm SLA 3 lần" : "SLA Violation — lead giao lại",
                cancellationToken);

            await _notificationService.NotifySlaViolationAsync(
                request.ViolatedAssigneeId, request.CustomerId, customerName, cancellationToken);

            await _emailSender.SendEmailAsync(
                $"{request.ViolatedAssigneeId}@system.local",
                "CẢNH BÁO: Vi phạm SLA Khách hàng",
                $"<p>Bạn đã vi phạm SLA khi không liên hệ khách hàng {customerName} trong thời gian quy định. Lead này đã bị hệ thống thu hồi tự động.</p>",
                cancellationToken);

            _logger.LogInformation(
                "SLA Reassigned: KH {CustomerName} ({CustomerId}) từ NV {OldAssignee} sang {NewAssignee}",
                customerName, request.CustomerId, request.ViolatedAssigneeId, newConsultantName);

            return Result<Guid?>.Success(nextAssigneeId.Value);
        }
    }
}
