using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;
using System.Collections.Generic;

namespace Auth.Application.Features.Authentication.Queries.GetTeams;

public class GetTeamsQuery : IRequest<List<TeamDto>>
{
}
