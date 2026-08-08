using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Queries.GetUsers;

public class GetUsersQuery : IRequest<Result<List<UserDto>>>
{
}
