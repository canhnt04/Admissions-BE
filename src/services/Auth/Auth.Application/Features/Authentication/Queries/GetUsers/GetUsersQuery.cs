using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;

namespace Auth.Application.Features.Authentication.Queries.GetUsers;

public class GetUsersQuery : IRequest<List<UserDto>>
{
}
