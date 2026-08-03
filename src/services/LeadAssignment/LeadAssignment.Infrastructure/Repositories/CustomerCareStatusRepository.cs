using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using LeadAssignment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LeadAssignment.Infrastructure.Repositories
{
    public class CustomerCareStatusRepository : GenericRepository<CustomerCareStatus, AssignmentDbContext>, ICustomerCareStatusRepository
    {
        public CustomerCareStatusRepository(AssignmentDbContext dbContext) : base(dbContext) { }

        public async Task<CustomerCareStatus?> GetActiveAsync(
            Guid customerId,
            Guid assigneeId,
            CancellationToken cancellationToken = default)
            => await _dbSet.FirstOrDefaultAsync(
                s => s.CustomerId == customerId &&
                     s.AssigneeId == assigneeId &&
                     !s.IsContactMade &&
                     !s.IsReassigned,
                cancellationToken);

        public async Task<CustomerCareStatus?> GetLatestActiveAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
            => await _dbSet
                .Where(s => s.CustomerId == customerId && !s.IsReassigned)
                .OrderByDescending(s => s.AssignedAt)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<int> CountSlaViolationsAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
            => await _dbSet.CountAsync(
                s => s.CustomerId == customerId && s.IsViolated,
                cancellationToken);
    }
}
