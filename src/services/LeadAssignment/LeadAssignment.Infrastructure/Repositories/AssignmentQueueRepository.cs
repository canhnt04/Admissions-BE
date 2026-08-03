using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using LeadAssignment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Repositories;
using Shared.Contracts.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LeadAssignment.Infrastructure.Repositories
{
    public class AssignmentQueueRepository : GenericRepository<AssignmentQueue, AssignmentDbContext>, IAssignmentQueueRepository
    {
        public AssignmentQueueRepository(AssignmentDbContext dbContext) : base(dbContext) { }

        public async Task<AssignmentQueue?> GetNextInQueueAsync(
            TrainingSystem? trainingSystem,
            Guid? excludeConsultantId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(q => q.TrainingSystem == trainingSystem &&
                            q.IsActive &&
                            q.CurrentLoad < q.MaxLoad);

            if (excludeConsultantId.HasValue)
                query = query.Where(q => q.ConsultantId != excludeConsultantId.Value);

            return await query
                .OrderBy(q => q.LastAssignedAt ?? DateTime.MinValue)
                .ThenBy(q => q.OrderIndex)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AssignmentQueue?> GetByConsultantAndSystemAsync(
            Guid consultantId,
            TrainingSystem? trainingSystem,
            CancellationToken cancellationToken = default)
            => await _dbSet.FirstOrDefaultAsync(
                q => q.ConsultantId == consultantId && q.TrainingSystem == trainingSystem,
                cancellationToken);
    }
}
