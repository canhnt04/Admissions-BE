using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using Auth.Domain.Entities;
using MediatR;
using Shared.Common;

namespace Auth.Application.Features.Authentication.Commands.AssignUser;

public class AssignUserCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public Role? Role { get; set; }
    public Guid? TeamId { get; set; }
}
