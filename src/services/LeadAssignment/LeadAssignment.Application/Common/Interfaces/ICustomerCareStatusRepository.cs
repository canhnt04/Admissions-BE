using LeadAssignment.Domain.Entities;
using Shared.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LeadAssignment.Application.Common.Interfaces
{
    public interface ICustomerCareStatusRepository : IRepository<CustomerCareStatus>
    {
        /// <summary>
        /// Lấy SLA tracking đang active (chưa liên hệ, chưa bị thu hồi) cho 1 KH + NV.
        /// </summary>
        Task<CustomerCareStatus?> GetActiveAsync(Guid customerId, Guid assigneeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy lần giao gần nhất (chưa bị reassigned) cho 1 KH.
        /// </summary>
        Task<CustomerCareStatus?> GetLatestActiveAsync(Guid customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Đếm số lần KH đã vi phạm SLA.
        /// </summary>
        Task<int> CountSlaViolationsAsync(Guid customerId, CancellationToken cancellationToken = default);
    }
}
