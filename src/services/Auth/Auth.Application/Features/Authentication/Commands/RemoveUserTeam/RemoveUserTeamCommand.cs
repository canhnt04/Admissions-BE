using MediatR;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Commands.RemoveUserTeam;

public class RemoveUserTeamCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}
