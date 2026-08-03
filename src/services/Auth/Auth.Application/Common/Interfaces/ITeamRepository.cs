using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Shared.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Auth.Application.Common.Interfaces
{
    /// <summary>
    /// Repository interface cho Team entity — Auth service.
    /// </summary>
    public interface ITeamRepository : IRepository<Team>
    {
        Task<Team?> GetByRoleTeamAsync(RoleTeam roleTeam, CancellationToken cancellationToken = default);
    }
}
