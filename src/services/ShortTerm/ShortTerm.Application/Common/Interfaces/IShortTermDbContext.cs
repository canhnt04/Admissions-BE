using ShortTerm.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using ShortTerm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ShortTerm.Application.Common.Interfaces
{
    public interface IShortTermDbContext
    {
        DbSet<ShortTerm.Domain.Entities.Customer> Customers { get; }
        DbSet<Course> Courses { get; }
        DbSet<CourseParticipant> CourseParticipants { get; }
        DbSet<CourseParticipantPayment> CourseParticipantPayments { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}


