using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction cho CrmDbContext — cho phép Application layer truy cập DB
    /// mà không phụ thuộc trực tiếp vào Infrastructure.
    /// </summary>
    public interface ICrmDbContext
    {
        // ─── Existing ───
        DbSet<Customer> Customers { get; set; }
        DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<Course> Courses { get; set; }
        DbSet<CourseParticipant> CourseParticipants { get; set; }
        DbSet<CourseParticipantPayment> CourseParticipantPayments { get; set; }
        DbSet<AuditLog> AuditLogs { get; set; }

        // ─── New (Auto-Assignment & SLA) ───
        DbSet<Team> Teams { get; set; }
        DbSet<CustomTag> CustomTags { get; set; }
        DbSet<ContactEvidence> ContactEvidences { get; set; }
        DbSet<AssignmentQueue> AssignmentQueues { get; set; }
        DbSet<SlaTracking> SlaTrackings { get; set; }
        DbSet<Notification> Notifications { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
