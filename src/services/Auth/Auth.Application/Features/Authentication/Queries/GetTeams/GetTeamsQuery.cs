using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;
using System.Collections.Generic;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Queries.GetTeams;

public class GetTeamsQuery : IRequest<Result<List<TeamDto>>>
{
}
