using LeadAssignment.Application.Common.Interfaces;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LeadAssignment.Application.Assignments.Queries.GetCustomerAssignmentHistory
{
    public class GetCustomerAssignmentHistoryQueryHandler : IRequestHandler<GetCustomerAssignmentHistoryQuery, Result<List<CustomerAssignmentHistoryDto>>>
    {
        private readonly IAssignmentDbContext _context;

        public GetCustomerAssignmentHistoryQueryHandler(IAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CustomerAssignmentHistoryDto>>> Handle(GetCustomerAssignmentHistoryQuery request, CancellationToken cancellationToken)
        {
            var histories = await _context.CustomerAssignmentHistories
                .Where(x => x.CustomerId == request.CustomerId)
                .OrderByDescending(x => x.AssignmentDate)
                .ToListAsync(cancellationToken);

            var assigneeIds = histories.Select(x => x.AssigneeId).Distinct().ToList();
            var assignees = await _context.UserReplicas
                .Where(x => assigneeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

            var result = histories.Select(h => new CustomerAssignmentHistoryDto
            {
                Id = h.Id,
                AssigneeId = h.AssigneeId,
                AssigneeName = assignees.ContainsKey(h.AssigneeId) ? assignees[h.AssigneeId] : "N/A",
                AssignedById = h.AssignedById,
                AssignmentDate = h.AssignmentDate,
                Reason = h.Reason.ToString(),
                Note = h.Note
            }).ToList();

            return Result<List<CustomerAssignmentHistoryDto>>.Success(result);
        }
    }
}

