using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;

using LeadAssignment.Domain.Entities;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using Microsoft.Extensions.Logging;

namespace LeadAssignment.Application.Assignments.Commands.CheckOut
{
    public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result<bool>>
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAssignmentDbContext _context;
        private readonly Microsoft.Extensions.Logging.ILogger<CheckOutCommandHandler> _logger;

        public CheckOutCommandHandler(
            IAuditLogRepository auditLogRepository, 
            IAssignmentDbContext context,
            Microsoft.Extensions.Logging.ILogger<CheckOutCommandHandler> logger)
        {
            _auditLogRepository = auditLogRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(CheckOutCommand request, CancellationToken cancellationToken)
        {
            var latestLog = await _auditLogRepository.Query()
                .Where(a => a.UserId == request.ConsultantId && a.RecordEntity == RecordEntity.User && 
                       a.Action == LeadAssignment.Domain.Enums.Action.Update &&
                       (a.Detail.Contains("CHECK_IN") || a.Detail.Contains("CHECK_OUT")))
                .OrderByDescending(a => a.CreationDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestLog == null || latestLog.Detail.Contains("CHECK_OUT"))
            {
                _logger.LogWarning("Spam check-out detected: Consultant {ConsultantId} is not currently checked in.", request.ConsultantId);
                return Result<bool>.Failure(new Error(400, "Assignment.SpamCheckOut", "Bạn chưa check-in hoặc đã check-out rồi. Vui lòng không thực hiện lại thao tác này!"));
            }

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Enums.Action.Update,
                Detail = $"[CHECK_OUT] Nhân viên {request.ConsultantId} check-out.",
                RecordId = request.ConsultantId,
                RecordDesc = request.ConsultantId.ToString(),
                RecordEntity = RecordEntity.User,
                CreationDate = Shared.Common.Helpers.TimeHelper.VietnamNow,
                UserId = request.ConsultantId
            };
            
            _auditLogRepository.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
