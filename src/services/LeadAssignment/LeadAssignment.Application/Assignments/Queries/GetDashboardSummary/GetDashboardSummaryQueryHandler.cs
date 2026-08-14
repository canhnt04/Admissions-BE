using Customer.Domain.Enums;
using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shared.Contracts.Enums;

namespace LeadAssignment.Application.Assignments.Queries.GetDashboardSummary
{
    public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
    {
        private readonly IAssignmentDbContext _context;

        public GetDashboardSummaryQueryHandler(IAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var rawSlaList = await _context.CustomerCareStatuses
                .Where(s => s.Status == null || s.Status == LeadStatus.New)
                .Select(s => new
                {
                    s.AssigneeId,
                    s.TrainingSystem,
                    s.StatusDate
                })
                .ToListAsync(cancellationToken);

            var now = Shared.Common.Helpers.TimeHelper.VietnamNow;
            var slaSettings = new LeadAssignment.Application.Common.Models.SlaSettings
            {
                SlaDeadlineMinutes = 30,
                AdminSlaDeadlineMinutes = 120,
                MaxSlaMultiplier = 4
            };

            var dto = new DashboardSummaryDto();
            dto.Kpis.TotalLeads = rawSlaList.Count;
            dto.Kpis.UnassignedLeads = rawSlaList.Count(x => x.AssigneeId == null);

            var assignedLeads = rawSlaList.Where(x => x.AssigneeId != null).ToList();
            foreach (var s in assignedLeads)
            {
                var baseSlaMins = slaSettings.SlaDeadlineMinutes;
                int currentLoad = assignedLeads.Count(x => x.AssigneeId == s.AssigneeId && x.TrainingSystem == s.TrainingSystem);
                int multiplier = Math.Min(slaSettings.MaxSlaMultiplier, Math.Max(1, currentLoad));
                
                var assignedAt = s.StatusDate ?? now;
                var deadline = assignedAt.AddMinutes(baseSlaMins * multiplier);

                if (deadline < now)
                {
                    dto.Kpis.OverdueSla++;
                }
                else
                {
                    dto.Kpis.ActiveSla++;
                }
            }

            dto.Branches.Formal = rawSlaList.Count(x => x.TrainingSystem == TrainingSystem.Formal);
            dto.Branches.Driving = rawSlaList.Count(x => x.TrainingSystem == TrainingSystem.Driving);
            dto.Branches.ShortTerm = rawSlaList.Count(x => x.TrainingSystem == TrainingSystem.ShortTerm);

            return Result<DashboardSummaryDto>.Success(dto);
        }
    }
}
