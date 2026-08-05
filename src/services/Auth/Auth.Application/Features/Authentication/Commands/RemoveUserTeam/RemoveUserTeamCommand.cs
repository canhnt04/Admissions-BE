using MediatR;

namespace Auth.Application.Features.Authentication.Commands.RemoveUserTeam;

public class RemoveUserTeamCommand : IRequest<RemoveUserTeamResponse>
{
    public Guid UserId { get; set; }
}

public class RemoveUserTeamResponse
{
    public string Message { get; set; } = string.Empty;
}
