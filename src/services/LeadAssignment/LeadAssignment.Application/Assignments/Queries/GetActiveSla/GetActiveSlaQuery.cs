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
        public string AssigneeName { get; set; } = "Unknown";
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
                .Where(s => !s.IsContactMade && !s.IsReassigned);

            if (request.TrainingSystem.HasValue)
                query = query.Where(s => s.TrainingSystem == request.TrainingSystem.Value);

            var rawSlaList = await query
                .OrderBy(s => s.Deadline)
                .Select(s => new
                {
                    s.Id,
                    s.CustomerId,
                    s.CustomerName,
                    s.TrainingSystem,
                    s.AssigneeId,
                    s.AssignedAt,
                    s.Deadline,
                    s.IsViolated,
                })
                .ToListAsync(cancellationToken);

            var assigneeIds = rawSlaList.Select(s => s.AssigneeId).Distinct().ToList();
            var userNames = await _userGrpcClient.GetUserNamesAsync(assigneeIds, cancellationToken);

            var now = DateTime.UtcNow;
            var result = rawSlaList.Select(s => new ActiveSlaDto
            {
                Id = s.Id,
                CustomerId = s.CustomerId,
                CustomerName = s.CustomerName,
                TrainingSystem = s.TrainingSystem.ToString() ?? string.Empty,
                AssigneeId = s.AssigneeId,
                AssigneeName = userNames.GetValueOrDefault(s.AssigneeId, "Unknown"),
                AssignedAt = s.AssignedAt,
                Deadline = s.Deadline,
                RemainingMinutes = (int)(s.Deadline - now).TotalMinutes,
                IsViolated = s.IsViolated,
            }).ToList();

            return Result<List<ActiveSlaDto>>.Success(result);
        }
    }
}
