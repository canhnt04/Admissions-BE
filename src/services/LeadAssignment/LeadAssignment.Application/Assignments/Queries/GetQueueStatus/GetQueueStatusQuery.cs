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
        public Guid? ConsultantId { get; set; }
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
            // Retrieve recent check-in/check-out state from AuditLog.
            var tenDaysAgo = Shared.Common.Helpers.TimeHelper.VietnamNow.AddDays(-10);
            var rawLogs = await _auditLogRepository.Query()
                .Where(a => a.RecordEntity == RecordEntity.User && 
                       a.Action == LeadAssignment.Domain.Enums.Action.Update &&
                       (a.Detail.Contains("CHECK_IN") || a.Detail.Contains("CHECK_OUT")) &&
                       a.CreationDate > tenDaysAgo)
                .ToListAsync(cancellationToken);

            var activeLogs = rawLogs
                .GroupBy(a => a.UserId)
                .Select(g => g.OrderByDescending(x => x.CreationDate).FirstOrDefault())
                .ToList();

            var activeConsultantLogs = activeLogs
                .Where(l => l != null && l.Detail.Contains("CHECK_IN"))
                .Select(l => l!);

            if (request.TrainingSystem.HasValue)
            {
                var systemStr = request.TrainingSystem.Value.ToString();
                activeConsultantLogs = activeConsultantLogs.Where(l => l.Detail.Contains(systemStr));
            }

            var activeConsultantIds = activeConsultantLogs
                .Select(l => l.UserId)
                .Distinct()
                .ToList();

            List<Guid> consultantIds;

            if (request.ConsultantId.HasValue)
            {
                // Only get for the specific consultant
                consultantIds = new List<Guid> { request.ConsultantId.Value };
            }
            else
            {
                consultantIds = activeConsultantIds;
            }
                
            var fullNames = await _userGrpcClient.GetUserFullNamesAsync(consultantIds, cancellationToken);
            
            var result = new List<QueueStatusDto>();
            
            // For each active consultant, calculate their current load
            foreach (var cid in consultantIds)
            {
                var query = _customerCareStatusRepository.Query()
                    .Where(c => c.AssigneeId == cid && (c.Status == null || c.Status == LeadStatus.New));
                    
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
                    TrainingSystem = request.TrainingSystem?.ToString(),
                    ConsultantId = cid,
                    ConsultantName = fullNames.TryGetValue(cid, out var name) && !string.IsNullOrEmpty(name) ? name : $"Nhân viên ({cid.ToString()[..8]})",
                    OrderIndex = 0,
                    CurrentLoad = currentLoad,
                    MaxLoad = 10,
                    IsActive = activeConsultantIds.Contains(cid), 
                    LastAssignedAt = lastAssignment?.AssignmentDate
                });
            }

            // Sort queue list by LastAssignedAt (nulls/waiting longest first) and set OrderIndex
            var sorted = result
                .OrderBy(x => x.LastAssignedAt.HasValue)
                .ThenBy(x => x.LastAssignedAt)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].OrderIndex = i + 1;
            }

            return Result<List<QueueStatusDto>>.Success(sorted);
        }
    }
}
