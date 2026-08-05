using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace LeadAssignment.Application.Common.Interfaces
{
    public interface IAssignmentDbContext
    {
        DbSet<AuditLog> AuditLogs { get; set; }

        DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
