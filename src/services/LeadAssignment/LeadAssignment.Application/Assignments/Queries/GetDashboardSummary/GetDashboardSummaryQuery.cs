using MediatR;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Queries.GetDashboardSummary
{
    public class GetDashboardSummaryQuery : IRequest<Result<DashboardSummaryDto>>
    {
    }
}
