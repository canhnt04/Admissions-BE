using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;

using LeadAssignment.Domain.Entities;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using Microsoft.Extensions.Logging;

namespace LeadAssignment.Application.Assignments.Commands.CheckIn
{
    public class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result<bool>>
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAssignmentDbContext _context;
        private readonly IMediator _mediator;
        private readonly Microsoft.Extensions.Logging.ILogger<CheckInCommandHandler> _logger;

        public CheckInCommandHandler(
            IAuditLogRepository auditLogRepository, 
            IAssignmentDbContext context, 
            IMediator mediator,
            Microsoft.Extensions.Logging.ILogger<CheckInCommandHandler> logger)
        {
            _auditLogRepository = auditLogRepository;
            _context = context;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(CheckInCommand request, CancellationToken cancellationToken)
        {
            var latestLog = await _auditLogRepository.Query()
                .Where(a => a.UserId == request.ConsultantId && a.RecordEntity == RecordEntity.User && 
                       a.Action == LeadAssignment.Domain.Enums.Action.Update &&
                       (a.Detail.Contains("CHECK_IN") || a.Detail.Contains("CHECK_OUT")))
                .OrderByDescending(a => a.CreationDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestLog != null && latestLog.Detail.Contains("CHECK_IN"))
            {
                _logger.LogWarning("Spam check-in detected: Consultant {ConsultantId} is already checked in for {TrainingSystem}.", request.ConsultantId, request.TrainingSystem);
                return Result<bool>.Failure(new Error(400, "Assignment.SpamCheckIn", "Bạn đã check-in rồi. Vui lòng không thực hiện lại thao tác này!"));
            }

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Enums.Action.Update,
                Detail = $"[CHECK_IN] Nhân viên {request.ConsultantId} check-in nhánh {request.TrainingSystem}.",
                RecordId = request.ConsultantId,
                RecordDesc = request.ConsultantId.ToString(),
                RecordEntity = RecordEntity.User,
                CreationDate = Shared.Common.Helpers.TimeHelper.VietnamNow,
                UserId = request.ConsultantId
            };
            
            _auditLogRepository.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            // Trigger Assignment Engine
            await _mediator.Send(new LeadAssignment.Application.Assignments.Commands.AssignPendingLeads.AssignPendingLeadsCommand 
            { 
                TrainingSystem = request.TrainingSystem 
            }, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
