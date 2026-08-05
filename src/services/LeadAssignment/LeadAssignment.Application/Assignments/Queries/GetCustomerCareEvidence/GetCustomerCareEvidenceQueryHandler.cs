using MediatR;
using Shared.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeadAssignment.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LeadAssignment.Application.Assignments.Queries.GetCustomerCareEvidence
{
    public class GetCustomerCareEvidenceQueryHandler : IRequestHandler<GetCustomerCareEvidenceQuery, Result<List<CustomerCareEvidenceDto>>>
    {
        private readonly IAssignmentDbContext _context;

        public GetCustomerCareEvidenceQueryHandler(IAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CustomerCareEvidenceDto>>> Handle(GetCustomerCareEvidenceQuery request, CancellationToken cancellationToken)
        {
            var statuses = await _context.CustomerCareStatuses
                .Where(x => x.CustomerId == request.CustomerId)
                .OrderByDescending(x => x.StatusDate)
                .Select(x => new CustomerCareEvidenceDto
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
                    TrainingSystem = x.TrainingSystem,
                    AssigneeId = x.AssigneeId,
                    Status = x.Status,
                    FollowStatus = x.FollowStatus,
                    StatusDate = x.StatusDate,
                    ReportDate = x.ReportDate,
                    Note = x.Note
                })
                .ToListAsync(cancellationToken);

            return Result<List<CustomerCareEvidenceDto>>.Success(statuses);
        }
    }
}
