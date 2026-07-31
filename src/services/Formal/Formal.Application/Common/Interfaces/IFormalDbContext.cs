using Formal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Formal.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction cho CRM Branch DbContext — dùng cho cả 3 nhánh (Formal, ShortTerm, Driving).
    /// Không chứa User/Team — chúng nằm trong Auth service.
    /// </summary>
    public interface IFormalDbContext
    {
        // ─── Bản sao User (đồng bộ qua Event Driven) ───
        DbSet<UserReplica> UserReplicas { get; set; }

        // ─── CRM Entities ───
        DbSet<Customer> Customers { get; set; }
        DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }
        DbSet<Course> Courses { get; set; }
        DbSet<CourseParticipant> CourseParticipants { get; set; }
        DbSet<CourseParticipantPayment> CourseParticipantPayments { get; set; }
        DbSet<AuditLog> AuditLogs { get; set; }

        // ─── Auto-Assignment & SLA ───
        DbSet<CustomTag> CustomTags { get; set; }
        DbSet<ContactEvidence> ContactEvidences { get; set; }
        DbSet<AssignmentQueue> AssignmentQueues { get; set; }
        DbSet<SlaTracking> SlaTrackings { get; set; }
        DbSet<Notification> Notifications { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

