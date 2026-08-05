using Formal.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using Formal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Formal.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction cho Formal DbContext.
    /// Chỉ chứa các entity thuộc nghiệp vụ Formal.
    /// </summary>
    public interface IFormalDbContext
    {
        // ─── Bản sao User (đồng bộ qua Event Driven) ───

        // ─── CRM Entities ───
        DbSet<Formal.Domain.Entities.Customer> Customers { get; set; }
        DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }
        DbSet<Course> Courses { get; set; }
        DbSet<CourseParticipant> CourseParticipants { get; set; }
        DbSet<CourseParticipantPayment> CourseParticipantPayments { get; set; }
        DbSet<AuditLog> AuditLogs { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
