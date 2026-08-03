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
        DbSet<CustomTag> CustomTags { get; set; }
        DbSet<ContactEvidence> ContactEvidences { get; set; }
        DbSet<AssignmentQueue> AssignmentQueues { get; set; }
        DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }
        DbSet<Notification> Notifications { get; set; }
        DbSet<SystemConfig> SystemConfigs { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

