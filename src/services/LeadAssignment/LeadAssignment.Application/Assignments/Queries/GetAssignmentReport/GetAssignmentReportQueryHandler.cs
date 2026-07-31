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

        public GetAssignmentReportQueryHandler(IAssignmentDbContext context)
        {
            _context = context;
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

            var consultantIds = grouped.Select(x => x.ConsultantId).ToList();
            var consultants = await _context.UserReplicas
                .Where(x => consultantIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

            var report = grouped.Select(g => new AssignmentReportDto
            {
                ConsultantId = g.ConsultantId,
                ConsultantName = consultants.ContainsKey(g.ConsultantId) ? consultants[g.ConsultantId] : "N/A",
                TotalAssigned = g.TotalAssigned,
                SlaFulfilled = g.SlaFulfilled,
                SlaViolated = g.SlaViolated,
                Pending = g.Pending
            }).ToList();

            return Result<List<AssignmentReportDto>>.Success(report);
        }
    }
}

