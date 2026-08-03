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
        public string ConsultantName { get; set; } = "Unknown";
        public int OrderIndex { get; set; }
        public int CurrentLoad { get; set; }
        public int MaxLoad { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastAssignedAt { get; set; }
    }

    public class GetQueueStatusQueryHandler : IRequestHandler<GetQueueStatusQuery, Result<List<QueueStatusDto>>>
    {
        private readonly IAssignmentDbContext _context;
        private readonly IUserGrpcClient _userGrpcClient;

        public GetQueueStatusQueryHandler(IAssignmentDbContext context, IUserGrpcClient userGrpcClient)
        {
            _context = context;
            _userGrpcClient = userGrpcClient;
        }

        public async Task<Result<List<QueueStatusDto>>> Handle(GetQueueStatusQuery request, CancellationToken cancellationToken)
        {
            var query = _context.AssignmentQueues.AsQueryable();

            if (request.TrainingSystem.HasValue)
                query = query.Where(q => q.TrainingSystem == request.TrainingSystem.Value);

            var rawQueue = await query
                .OrderBy(q => q.TrainingSystem)
                .ThenBy(q => q.OrderIndex)
                .Select(q => new
                {
                    q.Id,
                    q.TrainingSystem,
                    q.ConsultantId,
                    q.OrderIndex,
                    q.CurrentLoad,
                    q.MaxLoad,
                    q.IsActive,
                    q.LastAssignedAt,
                })
                .ToListAsync(cancellationToken);

            var consultantIds = rawQueue.Select(q => q.ConsultantId).Distinct().ToList();
            var userNames = await _userGrpcClient.GetUserNamesAsync(consultantIds, cancellationToken);

            var result = rawQueue.Select(q => new QueueStatusDto
            {
                Id = q.Id,
                TrainingSystem = q.TrainingSystem.ToString() ?? string.Empty,
                ConsultantId = q.ConsultantId,
                ConsultantName = userNames.GetValueOrDefault(q.ConsultantId, "Unknown"),
                OrderIndex = q.OrderIndex,
                CurrentLoad = q.CurrentLoad,
                MaxLoad = q.MaxLoad,
                IsActive = q.IsActive,
                LastAssignedAt = q.LastAssignedAt,
            }).ToList();

            return Result<List<QueueStatusDto>>.Success(result);
        }
    }
}
