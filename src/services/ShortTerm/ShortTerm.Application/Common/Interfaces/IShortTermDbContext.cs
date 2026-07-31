using ShortTerm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ShortTerm.Application.Common.Interfaces
{
    public interface IShortTermDbContext
    {
        DbSet<Customer> Customers { get; }
        DbSet<Course> Courses { get; }
        DbSet<CourseParticipant> CourseParticipants { get; }
        DbSet<CourseParticipantPayment> CourseParticipantPayments { get; }
        DbSet<CustomTag> CustomTags { get; }
        DbSet<UserReplica> UserReplicas { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

