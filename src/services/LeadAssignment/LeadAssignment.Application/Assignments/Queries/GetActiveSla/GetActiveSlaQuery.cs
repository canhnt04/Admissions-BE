using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using System.Collections.Generic;

namespace LeadAssignment.Application.Assignments.Queries.GetActiveSla
{
    public class GetActiveSlaQuery : IRequest<Result<List<ActiveSlaDto>>>
    {
        public TrainingSystem? TrainingSystem { get; set; }
    }

    public class ActiveSlaDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string TrainingSystem { get; set; } = string.Empty;
        public Guid AssigneeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public DateTime Deadline { get; set; }
        public int RemainingMinutes { get; set; }
        public bool IsViolated { get; set; }
    }

    public class GetActiveSlaQueryHandler : IRequestHandler<GetActiveSlaQuery, Result<List<ActiveSlaDto>>>
    {
        private readonly IAssignmentDbContext _context;
        private readonly IUserGrpcClient _userGrpcClient;

        public GetActiveSlaQueryHandler(IAssignmentDbContext context, IUserGrpcClient userGrpcClient)
        {
            _context = context;
            _userGrpcClient = userGrpcClient;
        }

        public async Task<Result<List<ActiveSlaDto>>> Handle(GetActiveSlaQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CustomerCareStatuses
                .Where(s => s.AssigneeId != null && s.Status == null);

            if (request.TrainingSystem.HasValue)
                query = query.Where(s => s.TrainingSystem == request.TrainingSystem.Value);

            var rawSlaList = await query
                .OrderBy(s => s.StatusDate)
                .Select(s => new
                {
                    s.Id,
                    s.CustomerId,
                    s.CustomerName,
                    s.TrainingSystem,
                    AssigneeId = s.AssigneeId.Value,
                    StatusDate = s.StatusDate
                })
                .ToListAsync(cancellationToken);

            var assigneeIds = rawSlaList.Select(s => s.AssigneeId).Distinct().ToList();
            var userNames = await _userGrpcClient.GetUserNamesAsync(assigneeIds, cancellationToken);

            var now = DateTime.UtcNow;
            var slaThreshold = now.AddMinutes(-30);
            
            var result = rawSlaList.Select(s => new ActiveSlaDto
            {
                Id = s.Id,
                CustomerId = s.CustomerId,
                CustomerName = s.CustomerName,
                TrainingSystem = s.TrainingSystem.ToString() ?? string.Empty,
                AssigneeId = s.AssigneeId,
                AssigneeName = userNames[s.AssigneeId],
                AssignedAt = s.StatusDate ?? now,
                Deadline = (s.StatusDate ?? now).AddMinutes(30),
                RemainingMinutes = (int)((s.StatusDate ?? now).AddMinutes(30) - now).TotalMinutes,
                IsViolated = s.StatusDate < slaThreshold,
            }).ToList();

            return Result<List<ActiveSlaDto>>.Success(result);
        }
    }
}
