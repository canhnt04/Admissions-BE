using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using System.Collections.Generic;

namespace LeadAssignment.Application.Assignments.Queries.GetQueueStatus
{
    public class GetQueueStatusQuery : IRequest<Result<List<QueueStatusDto>>>
    {
        public TrainingSystem? TrainingSystem { get; set; }
    }

    public class QueueStatusDto
    {
        public Guid Id { get; set; }
        public string TrainingSystem { get; set; } = string.Empty;
        public Guid ConsultantId { get; set; }
        public string ConsultantName { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int CurrentLoad { get; set; }
        public int MaxLoad { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastAssignedAt { get; set; }
    }

    public class GetQueueStatusQueryHandler : IRequestHandler<GetQueueStatusQuery, Result<List<QueueStatusDto>>>
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ICustomerCareStatusRepository _customerCareStatusRepository;
        private readonly ICustomerAssignmentHistoryRepository _customerAssignmentHistoryRepository;
        private readonly IUserGrpcClient _userGrpcClient;

        public GetQueueStatusQueryHandler(
            IAuditLogRepository auditLogRepository, 
            ICustomerCareStatusRepository customerCareStatusRepository,
            ICustomerAssignmentHistoryRepository customerAssignmentHistoryRepository,
            IUserGrpcClient userGrpcClient)
        {
            _auditLogRepository = auditLogRepository;
            _customerCareStatusRepository = customerCareStatusRepository;
            _customerAssignmentHistoryRepository = customerAssignmentHistoryRepository;
            _userGrpcClient = userGrpcClient;
        }

        public async Task<Result<List<QueueStatusDto>>> Handle(GetQueueStatusQuery request, CancellationToken cancellationToken)
        {
            // Retrieve active consultants from AuditLog
            var tenDaysAgo = DateTime.UtcNow.AddDays(-10);
            var activeLogs = await _auditLogRepository.Query()
                .Where(a => a.RecordEntity == RecordEntity.User && 
                       a.Action == LeadAssignment.Domain.Enums.Action.Update &&
                       (a.Detail.Contains("CHECK_IN") || a.Detail.Contains("CHECK_OUT")) &&
                       a.CreationDate > tenDaysAgo)
                .GroupBy(a => a.UserId)
                .Select(g => g.OrderByDescending(x => x.CreationDate).FirstOrDefault())
                .ToListAsync(cancellationToken);

            var activeConsultantIds = activeLogs
                .Where(l => l != null && l.Detail.Contains("CHECK_IN"))
                .Select(l => l.UserId)
                .ToList();
                
            var userNames = await _userGrpcClient.GetUserNamesAsync(activeConsultantIds, cancellationToken);
            
            var result = new List<QueueStatusDto>();
            
            // For each active consultant, calculate their current load
            foreach (var cid in activeConsultantIds)
            {
                var query = _customerCareStatusRepository.Query()
                    .Where(c => c.AssigneeId == cid && c.Status == LeadStatus.New);
                    
                if (request.TrainingSystem.HasValue)
                {
                    query = query.Where(c => c.TrainingSystem == request.TrainingSystem.Value);
                }
                
                var currentLoad = await query.CountAsync(cancellationToken);
                
                var lastAssignment = await _customerAssignmentHistoryRepository.Query()
                    .Where(h => h.AssigneeId == cid)
                    .OrderByDescending(h => h.AssignmentDate)
                    .FirstOrDefaultAsync(cancellationToken);
                    
                result.Add(new QueueStatusDto
                {
                    Id = Guid.NewGuid(),
                    TrainingSystem = request.TrainingSystem?.ToString() ?? "All",
                    ConsultantId = cid,
                    ConsultantName = userNames[cid],
                    OrderIndex = 0,
                    CurrentLoad = currentLoad,
                    MaxLoad = 10,
                    IsActive = true,
                    LastAssignedAt = lastAssignment?.AssignmentDate
                });
            }

            return Result<List<QueueStatusDto>>.Success(result);
        }
    }
}
