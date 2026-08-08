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

namespace LeadAssignment.Application.Assignments.Queries.GetCustomerAssignmentHistory
{
    public class GetCustomerAssignmentHistoryQueryHandler : IRequestHandler<GetCustomerAssignmentHistoryQuery, Result<List<CustomerAssignmentHistoryDto>>>
    {
        private readonly IAssignmentDbContext _context;
        private readonly IUserGrpcClient _userGrpcClient;

        public GetCustomerAssignmentHistoryQueryHandler(IAssignmentDbContext context, IUserGrpcClient userGrpcClient)
        {
            _context = context;
            _userGrpcClient = userGrpcClient;
        }

        public async Task<Result<List<CustomerAssignmentHistoryDto>>> Handle(GetCustomerAssignmentHistoryQuery request, CancellationToken cancellationToken)
        {
            var histories = await _context.CustomerAssignmentHistories
                .Where(x => x.CustomerId == request.CustomerId)
                .OrderByDescending(x => x.AssignmentDate)
                .ToListAsync(cancellationToken);

            // Batch-resolve assignee names và assigned-by names qua gRPC
            var userIds = histories
                .SelectMany(h => new[] { h.AssigneeId, h.AssignedById })
                .Distinct()
                .Where(id => id != Guid.Empty)
                .ToList();

            var userInfos = await _userGrpcClient.GetUsersAsync(userIds, cancellationToken);

            var result = histories.Select(h => new CustomerAssignmentHistoryDto
            {
                Id = h.Id,
                AssigneeId = h.AssigneeId,
                AssigneeName = userInfos.TryGetValue(h.AssigneeId, out var info) ? info.FullName : string.Empty,
                AssignedById = h.AssignedById,
                AssignmentDate = h.AssignmentDate
            }).ToList();

            return Result<List<CustomerAssignmentHistoryDto>>.Success(result);
        }
    }
}
