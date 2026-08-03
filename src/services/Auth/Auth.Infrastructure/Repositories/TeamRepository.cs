using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Repositories
{
    public class TeamRepository : GenericRepository<Team, AuthDbContext>, ITeamRepository
    {
        public TeamRepository(AuthDbContext dbContext) : base(dbContext) { }

        public async Task<Team?> GetByRoleTeamAsync(RoleTeam roleTeam, CancellationToken cancellationToken = default)
            => await _dbSet.FirstOrDefaultAsync(t => t.RoleTeam == roleTeam, cancellationToken);
    }
}
