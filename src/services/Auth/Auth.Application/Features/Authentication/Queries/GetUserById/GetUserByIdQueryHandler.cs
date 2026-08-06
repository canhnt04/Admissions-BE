using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using Auth.Domain.Errors;
using Auth.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;

namespace Auth.Application.Features.Authentication.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Query()
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                Mobile = u.Mobile,
                IdentificationNumber = u.IdentificationNumber,
                Role = u.Role,
                TeamId = u.TeamId,
                RoleTeam = u.Team != null ? u.Team.RoleTeam : null,
                ProfilePicUrl = u.ProfilePicUrl,
                IsActived = u.IsActived,
                UserInternalId = u.UserInternalId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null) throw new NotFoundException(AuthErrors.UserNotFound);

        return user;
    }
}
