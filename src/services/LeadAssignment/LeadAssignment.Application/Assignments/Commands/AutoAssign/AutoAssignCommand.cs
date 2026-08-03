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

namespace LeadAssignment.Application.Assignments.Commands.AutoAssign
{
    public class AutoAssignCommand : IRequest<Result<Guid?>>
    {
        public Guid CustomerId { get; set; }
        public TrainingSystem? TrainingSystem { get; set; }
    }

    public class AutoAssignCommandHandler : IRequestHandler<AutoAssignCommand, Result<Guid?>>
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
        private readonly ILogger<AutoAssignCommandHandler> _logger;

        private const int DEFAULT_SLA_DEADLINE_MINUTES = 30;

        public AutoAssignCommandHandler(
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
            ILogger<AutoAssignCommandHandler> logger)
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

        public async Task<Result<Guid?>> Handle(AutoAssignCommand request, CancellationToken cancellationToken)
        {
            var nameRecord = await _customerCareStatusRepository.Query()
                .Where(x => x.CustomerId == request.CustomerId)
                .Select(x => x.CustomerName)
                .FirstOrDefaultAsync(cancellationToken);

            var customerName = nameRecord ?? "New Customer";

            var nextConsultant = await _assignmentQueueRepository.GetNextInQueueAsync(request.TrainingSystem, null, cancellationToken);

            if (nextConsultant == null)
            {
                _logger.LogWarning(
                    "Không tìm được NV nào trong queue cho nhánh {TrainingSystem}. KH {CustomerId} chưa được giao.",
                    request.TrainingSystem, request.CustomerId);
                return Result<Guid?>.Success(null);
            }

            var consultantNames = await _userGrpcClient.GetUserNamesAsync(
                new[] { nextConsultant.ConsultantId }, cancellationToken);
            var consultantName = consultantNames.GetValueOrDefault(nextConsultant.ConsultantId, "Unknown");
            var now = DateTime.UtcNow;
            var slaMinutes = await GetSlaDeadlineMinutesAsync(cancellationToken);
            var deadline = now.AddMinutes(slaMinutes);

            _customerAssignmentHistoryRepository.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                AssigneeId = nextConsultant.ConsultantId,
                AssignedById = Guid.Empty, // System
                AssignmentDate = now,
                Reason = AssignmentReason.NewLead,
                Note = $"Tự động giao lead (Round-Robin) cho {consultantName}",
            });

            _customerCareStatusRepository.Add(new CustomerCareStatus
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                CustomerName = customerName,
                TrainingSystem = request.TrainingSystem,
                AssigneeId = nextConsultant.ConsultantId,
                AssignedAt = now,
                Deadline = deadline,
                IsContactMade = false,
                IsViolated = false,
                IsReassigned = false,
            });

            nextConsultant.CurrentLoad += 1;
            nextConsultant.LastAssignedAt = now;
            _assignmentQueueRepository.Update(nextConsultant);

            _auditLogRepository.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Enums.Action.Insert,
                Detail = $"Auto-assign KH [{customerName}] cho NV [{consultantName}]. SLA deadline: {deadline:HH:mm:ss}",
                RecordId = request.CustomerId,
                RecordDesc = customerName,
                RecordEntity = RecordEntity.Customer,
                CreationDate = now,
                UserId = Guid.Empty,
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new LeadAssignedEvent
            {
                CustomerId = request.CustomerId,
                CustomerName = customerName,
                AssigneeId = nextConsultant.ConsultantId,
                AssigneeName = consultantName,
                AssignedById = Guid.Empty,
                Reason = AssignmentReason.NewLead,
                AssignedAt = now,
                SlaDeadline = deadline,
            }, cancellationToken);

            await _notificationService.NotifyLeadAssignedAsync(
                nextConsultant.ConsultantId, request.CustomerId, customerName, cancellationToken);

            await _emailSender.SendEmailAsync(
                $"{nextConsultant.ConsultantId}@system.local",
                "Bạn được giao khách hàng mới",
                $"<p>Chào bạn, bạn vừa được tự động phân bổ một khách hàng mới: {customerName}. Vui lòng liên hệ và chốt sales trước {deadline:HH:mm}!</p>",
                cancellationToken);

            _logger.LogInformation(
                "Auto-assigned KH {CustomerName} ({CustomerId}) cho NV {ConsultantName}. Deadline: {Deadline}",
                customerName, request.CustomerId, consultantName, deadline);

            return Result<Guid?>.Success(nextConsultant.ConsultantId);
        }
    }
}
