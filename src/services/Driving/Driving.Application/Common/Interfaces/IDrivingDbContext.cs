using Driving.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using Driving.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Driving.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction cho Driving DbContext.
    /// Chỉ chứa các entity thuộc nghiệp vụ Driving.
    /// </summary>
    public interface IDrivingDbContext
    {
        // ─── Bản sao User (đồng bộ qua Event Driven) ───

        // ─── CRM Entities ───
        DbSet<Driving.Domain.Entities.Customer> Customers { get; set; }
        DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }
        DbSet<Course> Courses { get; set; }
        DbSet<CourseParticipant> CourseParticipants { get; set; }
        DbSet<CourseParticipantPayment> CourseParticipantPayments { get; set; }
        DbSet<AuditLog> AuditLogs { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
