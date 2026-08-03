using LeadAssignment.Domain.Entities;
using Shared.Common.Interfaces;
using Shared.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeadAssignment.Application.Common.Interfaces
{
    public interface IAssignmentQueueRepository : IRepository<AssignmentQueue>
    {
        /// <summary>
        /// Lấy NV tiếp theo trong Round-Robin queue (active, chưa đầy tải).
        /// </summary>
        Task<AssignmentQueue?> GetNextInQueueAsync(TrainingSystem? trainingSystem, Guid? excludeConsultantId = null, CancellationToken cancellationToken = default);

        Task<AssignmentQueue?> GetByConsultantAndSystemAsync(Guid consultantId, TrainingSystem? trainingSystem, CancellationToken cancellationToken = default);
    }
}
