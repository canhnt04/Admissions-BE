using MediatR;

namespace Auth.Application.Features.Authentication.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<UserDto>
{
    public Guid UserId { get; set; }
}
