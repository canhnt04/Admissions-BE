using MediatR;

namespace Auth.Application.Features.Authentication.Queries.GetProfile;

public class GetProfileQuery : IRequest<UserDto>
{
    public Guid UserId { get; set; }
}
