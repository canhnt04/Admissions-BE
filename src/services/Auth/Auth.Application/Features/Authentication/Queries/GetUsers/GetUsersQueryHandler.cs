using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using Auth.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<List<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.Query()
            .AsNoTracking()
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
            .ToListAsync(cancellationToken);
        return Result<List<UserDto>>.Success(users);
    }
}
