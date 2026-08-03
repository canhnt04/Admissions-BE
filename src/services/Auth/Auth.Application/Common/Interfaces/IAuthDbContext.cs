using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Common.Interfaces;

public interface IAuthDbContext
{
    DbSet<User> Users { get; }
    DbSet<Team> Teams { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
