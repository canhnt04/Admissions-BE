using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using Auth.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Queries.GetTeams;

public class GetTeamsQueryHandler : IRequestHandler<GetTeamsQuery, Result<List<TeamDto>>>
{
    private readonly ITeamRepository _teamRepository;

    public GetTeamsQueryHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<Result<List<TeamDto>>> Handle(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        var teams = await _teamRepository.Query()
            .AsNoTracking()
            .Select(t => new TeamDto
            {
                Id = t.Id,
                Name = t.Name,
                RoleTeam = t.RoleTeam,
                IsActive = t.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result<List<TeamDto>>.Success(teams);
    }
}
