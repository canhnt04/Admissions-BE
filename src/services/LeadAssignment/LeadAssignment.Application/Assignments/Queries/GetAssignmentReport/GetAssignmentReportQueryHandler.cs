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
            var query = _context.CustomerCareStatuses.AsQueryable();

            if (request.FromDate.HasValue)
                query = query.Where(x => x.AssignedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(x => x.AssignedAt <= request.ToDate.Value);

            var grouped = await query
                .GroupBy(x => x.AssigneeId)
                .Select(g => new
                {
                    ConsultantId = g.Key,
                    TotalAssigned = g.Count(),
                    SlaFulfilled = g.Count(x => x.IsContactMade),
                    SlaViolated = g.Count(x => x.IsViolated),
                    Pending = g.Count(x => !x.IsContactMade && !x.IsViolated && !x.IsReassigned)
                })
                .ToListAsync(cancellationToken);

            // Lấy tên tư vấn viên qua gRPC
            var consultantIds = grouped.Select(g => g.ConsultantId).Distinct().ToList();
            var userNames = await _userGrpcClient.GetUserNamesAsync(consultantIds, cancellationToken);

            var report = grouped.Select(g => new AssignmentReportDto
            {
                ConsultantId = g.ConsultantId,
                ConsultantName = userNames.GetValueOrDefault(g.ConsultantId, "Unknown"),
                TotalAssigned = g.TotalAssigned,
                SlaFulfilled = g.SlaFulfilled,
                SlaViolated = g.SlaViolated,
                Pending = g.Pending
            }).ToList();

            return Result<List<AssignmentReportDto>>.Success(report);
        }
    }
}
