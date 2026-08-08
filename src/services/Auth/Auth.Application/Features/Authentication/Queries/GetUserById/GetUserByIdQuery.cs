using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
}
