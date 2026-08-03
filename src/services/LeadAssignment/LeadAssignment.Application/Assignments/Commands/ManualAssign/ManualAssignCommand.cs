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

namespace LeadAssignment.Application.Assignments.Commands.ManualAssign
{
    public class ManualAssignCommand : IRequest<Result<bool>>
    {
        public Guid CustomerId { get; set; }
        public Guid AssigneeId { get; set; }
        public Guid AssignedById { get; set; }
        public string? Note { get; set; }
    }

    public class ManualAssignCommandHandler : IRequestHandler<ManualAssignCommand, Result<bool>>
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
        private readonly ILogger<ManualAssignCommandHandler> _logger;

        private const int DEFAULT_SLA_DEADLINE_MINUTES = 30;

        public ManualAssignCommandHandler(
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
            ILogger<ManualAssignCommandHandler> logger)
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

        public async Task<Result<bool>> Handle(ManualAssignCommand request, CancellationToken cancellationToken)
        {
            var latestStatus = await _customerCareStatusRepository.GetLatestActiveAsync(request.CustomerId, cancellationToken);

            var customerName = latestStatus?.CustomerName ?? "Unknown Customer";
            var trainingSystem = latestStatus?.TrainingSystem ?? TrainingSystem.ShortTerm;

            // Resolve tên tư vấn viên qua gRPC
            var userNames = await _userGrpcClient.GetUserNamesAsync(
                new[] { request.AssigneeId, request.AssignedById }.Distinct(),
                cancellationToken);
            var assigneeName = userNames.GetValueOrDefault(request.AssigneeId, "Unknown");

            var now = DateTime.UtcNow;
            var slaMinutes = await GetSlaDeadlineMinutesAsync(cancellationToken);
            var deadline = now.AddMinutes(slaMinutes);

            if (latestStatus != null && latestStatus.AssigneeId != request.AssigneeId)
            {
                var oldQueue = await _assignmentQueueRepository.GetByConsultantAndSystemAsync(latestStatus.AssigneeId, trainingSystem, cancellationToken);
                if (oldQueue != null && oldQueue.CurrentLoad > 0)
                {
                    oldQueue.CurrentLoad -= 1;
                    _assignmentQueueRepository.Update(oldQueue);
                }

                latestStatus.IsReassigned = true;
                latestStatus.ReassignedAt = now;
                latestStatus.ReassignedToId = request.AssigneeId;
                _customerCareStatusRepository.Update(latestStatus);
            }

            _customerAssignmentHistoryRepository.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                AssigneeId = request.AssigneeId,
                AssignedById = request.AssignedById,
                AssignmentDate = now,
                Reason = AssignmentReason.ManualAssign,
                Note = request.Note ?? $"Giao thủ công bởi admin cho {assigneeName}",
            });

            _customerCareStatusRepository.Add(new CustomerCareStatus
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                CustomerName = customerName,
                TrainingSystem = trainingSystem,
                AssigneeId = request.AssigneeId,
                AssignedAt = now,
                Deadline = deadline,
                IsContactMade = false,
                IsViolated = false,
                IsReassigned = false,
            });

            var newQueue = await _assignmentQueueRepository.GetByConsultantAndSystemAsync(request.AssigneeId, trainingSystem, cancellationToken);
            if (newQueue != null)
            {
                newQueue.CurrentLoad += 1;
                newQueue.LastAssignedAt = now;
                _assignmentQueueRepository.Update(newQueue);
            }

            _auditLogRepository.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Enums.Action.Insert,
                Detail = $"Manual-assign KH [{customerName}] cho NV [{assigneeName}] bởi user {request.AssignedById}. Ghi chú: {request.Note}",
                RecordId = request.CustomerId,
                RecordDesc = customerName,
                RecordEntity = RecordEntity.Customer,
                CreationDate = now,
                UserId = request.AssignedById,
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new LeadAssignedEvent
            {
                CustomerId = request.CustomerId,
                CustomerName = customerName,
                AssigneeId = request.AssigneeId,
                AssigneeName = assigneeName,
                AssignedById = request.AssignedById,
                Reason = AssignmentReason.ManualAssign,
                AssignedAt = now,
                SlaDeadline = deadline,
            }, cancellationToken);

            await _notificationService.NotifyLeadAssignedAsync(request.AssigneeId, request.CustomerId, customerName, cancellationToken);
            await _emailSender.SendEmailAsync(
                $"{request.AssigneeId}@system.local",
                "Bạn được giao khách hàng thủ công",
                $"<p>Chào bạn, bạn được phân bổ thủ công một khách hàng mới: {customerName}. Vui lòng liên hệ và chốt sales trước {deadline:HH:mm}!</p>",
                cancellationToken);

            _logger.LogInformation(
                "Manual-assigned KH {CustomerName} ({CustomerId}) cho NV {AssigneeName} bởi {AssignedById}.",
                customerName, request.CustomerId, assigneeName, request.AssignedById);

            return Result<bool>.Success(true);
        }
    }
}
