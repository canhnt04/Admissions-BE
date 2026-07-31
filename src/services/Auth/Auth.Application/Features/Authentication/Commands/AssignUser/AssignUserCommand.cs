using Auth.Domain.Entities;
using MediatR;

namespace Auth.Application.Features.Authentication.Commands.AssignUser;

public class AssignUserCommand : IRequest<AssignUserResponse>
{
    public Guid UserId { get; set; }
    public Role Role { get; set; }
    public Guid? TeamId { get; set; }
}

public class AssignUserResponse
{
    public string Message { get; set; } = string.Empty;
}
