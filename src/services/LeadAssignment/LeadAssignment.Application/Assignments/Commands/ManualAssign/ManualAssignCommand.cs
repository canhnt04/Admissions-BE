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

        private readonly ICustomerAssignmentHistoryRepository _customerAssignmentHistoryRepository;
        private readonly Microsoft.Extensions.Options.IOptions<LeadAssignment.Application.Common.Models.SlaSettings> _slaSettings;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAssignmentDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly IEmailSender _emailSender;
        private readonly IUserGrpcClient _userGrpcClient;
        private readonly ILogger<ManualAssignCommandHandler> _logger;

        public ManualAssignCommandHandler(
            ICustomerCareStatusRepository customerCareStatusRepository,
            ICustomerAssignmentHistoryRepository customerAssignmentHistoryRepository,
            Microsoft.Extensions.Options.IOptions<LeadAssignment.Application.Common.Models.SlaSettings> slaSettings,
            IAuditLogRepository auditLogRepository,
            IAssignmentDbContext context,
            IPublishEndpoint publishEndpoint,
            IEmailSender emailSender,
            IUserGrpcClient userGrpcClient,
            ILogger<ManualAssignCommandHandler> logger)
        {
            _customerCareStatusRepository = customerCareStatusRepository;
            _customerAssignmentHistoryRepository = customerAssignmentHistoryRepository;
            _slaSettings = slaSettings;
            _auditLogRepository = auditLogRepository;
            _context = context;
            _publishEndpoint = publishEndpoint;
            _emailSender = emailSender;
            _userGrpcClient = userGrpcClient;
            _logger = logger;
        }



        public async Task<Result<bool>> Handle(ManualAssignCommand request, CancellationToken cancellationToken)
        {
            var latestStatus = await _customerCareStatusRepository.GetLatestActiveAsync(request.CustomerId, cancellationToken);

            var customerName = latestStatus?.CustomerName;
            var trainingSystem = latestStatus?.TrainingSystem;

            // Resolve tên tư vấn viên qua gRPC
            var fullNames = await _userGrpcClient.GetUserFullNamesAsync(
                new[] { request.AssigneeId, request.AssignedById }.Distinct(),
                cancellationToken);
            var assigneeName = fullNames[request.AssigneeId];

            var now = DateTime.UtcNow;

            int currentLoad = await _customerCareStatusRepository.Query()
                .CountAsync(c => c.AssigneeId == request.AssigneeId && c.Status == LeadStatus.New && c.TrainingSystem == trainingSystem, cancellationToken);
            
            int multiplier = Math.Min(_slaSettings.Value.MaxSlaMultiplier, Math.Max(1, currentLoad + 1));
            var dynamicSlaMinutes = _slaSettings.Value.SlaDeadlineMinutes * multiplier;
            var deadline = now.AddMinutes(dynamicSlaMinutes);

            if (latestStatus != null)
            {
                latestStatus.AssigneeId = request.AssigneeId;
                latestStatus.Status = LeadStatus.New;
                latestStatus.StatusDate = now;
                _customerCareStatusRepository.Update(latestStatus);
            }
            else
            {
                // In case there is no existing status
                _customerCareStatusRepository.Add(new CustomerCareStatus
                {
                    Id = Guid.NewGuid(),
                    CustomerId = request.CustomerId,
                    CustomerName = customerName,
                    TrainingSystem = trainingSystem,
                    AssigneeId = request.AssigneeId,
                    StatusDate = now
                });
            }

            _customerAssignmentHistoryRepository.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                AssigneeId = request.AssigneeId,
                AssignedById = request.AssignedById,
                AssignmentDate = now
            });



            _auditLogRepository.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Enums.Action.Assign,
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
