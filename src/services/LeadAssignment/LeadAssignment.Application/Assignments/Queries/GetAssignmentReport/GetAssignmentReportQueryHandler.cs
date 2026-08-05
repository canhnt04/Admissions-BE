using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LeadAssignment.Application.Assignments.Queries.GetAssignmentReport
{
    public class GetAssignmentReportQueryHandler : IRequestHandler<GetAssignmentReportQuery, Result<List<AssignmentReportDto>>>
    {
        private readonly IAssignmentDbContext _context;
        private readonly IUserGrpcClient _userGrpcClient;

        public GetAssignmentReportQueryHandler(IAssignmentDbContext context, IUserGrpcClient userGrpcClient)
        {
            _context = context;
            _userGrpcClient = userGrpcClient;
        }

        public async Task<Result<List<AssignmentReportDto>>> Handle(GetAssignmentReportQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CustomerCareStatuses.Where(x => x.AssigneeId != null).AsQueryable();

            if (request.FromDate.HasValue)
                query = query.Where(x => x.StatusDate >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(x => x.StatusDate <= request.ToDate.Value);

            // TODO: Fetch from config if needed. Using 30 mins as default
            var slaThreshold = System.DateTime.UtcNow.AddMinutes(-30);

            var grouped = await query
                .GroupBy(x => x.AssigneeId)
                .Select(g => new
                {
                    ConsultantId = g.Key!.Value,
                    TotalAssigned = g.Count(),
                    SlaFulfilled = g.Count(x => x.Status != LeadStatus.New),
                    SlaViolated = g.Count(x => x.Status == LeadStatus.New && x.StatusDate < slaThreshold),
                    Pending = g.Count(x => x.Status == LeadStatus.New && x.StatusDate >= slaThreshold)
                })
                .ToListAsync(cancellationToken);

            // Lấy tên tư vấn viên qua gRPC
            var consultantIds = grouped.Select(g => g.ConsultantId).Distinct().ToList();
            var fullNames = await _userGrpcClient.GetUserFullNamesAsync(consultantIds, cancellationToken);

            var report = grouped.Select(g => new AssignmentReportDto
            {
                ConsultantId = g.ConsultantId,
                ConsultantName = fullNames[g.ConsultantId],
                TotalAssigned = g.TotalAssigned,
                SlaFulfilled = g.SlaFulfilled,
                SlaViolated = g.SlaViolated,
                Pending = g.Pending
            }).ToList();

            return Result<List<AssignmentReportDto>>.Success(report);
        }
    }
}
