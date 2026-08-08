using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Queries.GetProfile;

public class GetProfileQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
}
