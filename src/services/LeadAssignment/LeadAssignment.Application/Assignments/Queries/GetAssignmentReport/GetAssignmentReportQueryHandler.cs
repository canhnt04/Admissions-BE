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

            var rawList = await query
                .Select(x => new
                {
                    AssigneeId = x.AssigneeId!.Value,
                    x.Status,
                    x.StatusDate,
                    x.TrainingSystem
                })
                .ToListAsync(cancellationToken);

            var now = Shared.Common.Helpers.TimeHelper.VietnamNow;
            var slaSettings = new LeadAssignment.Application.Common.Models.SlaSettings
            {
                SlaDeadlineMinutes = 30,
                AdminSlaDeadlineMinutes = 120,
                MaxSlaMultiplier = 4
            };

            var grouped = rawList.GroupBy(x => x.AssigneeId).ToList();

            // Lấy tên tư vấn viên qua gRPC
            var consultantIds = grouped.Select(g => g.Key).Distinct().ToList();
            var userInfos = await _userGrpcClient.GetUsersAsync(consultantIds, cancellationToken);

            var report = new List<AssignmentReportDto>();

            foreach (var g in grouped)
            {
                var consultantId = g.Key;
                var assignedCount = g.Count();
                var fulfilledCount = g.Count(x => x.Status != LeadStatus.New);
                
                int violatedCount = 0;
                int pendingCount = 0;

                var activeLeads = g.Where(x => x.Status == LeadStatus.New).ToList();
                foreach (var s in activeLeads)
                {
                    var baseSlaMins = slaSettings.SlaDeadlineMinutes;
                    int currentLoad = rawList.Count(x => x.AssigneeId == consultantId && x.TrainingSystem == s.TrainingSystem && x.Status == LeadStatus.New);
                    int multiplier = Math.Min(slaSettings.MaxSlaMultiplier, Math.Max(1, currentLoad));
                    
                    var assignedAt = s.StatusDate ?? now;
                    var deadline = assignedAt.AddMinutes(baseSlaMins * multiplier);

                    if (deadline < now)
                    {
                        violatedCount++;
                    }
                    else
                    {
                        pendingCount++;
                    }
                }

                report.Add(new AssignmentReportDto
                {
                    ConsultantId = consultantId,
                    ConsultantName = userInfos.TryGetValue(consultantId, out var info) ? info.FullName : string.Empty,
                    TotalAssigned = assignedCount,
                    SlaFulfilled = fulfilledCount,
                    SlaViolated = violatedCount,
                    Pending = pendingCount
                });
            }

            return Result<List<AssignmentReportDto>>.Success(report);
        }
    }
}
