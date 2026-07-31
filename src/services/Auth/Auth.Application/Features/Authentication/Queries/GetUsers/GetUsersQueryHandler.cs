using Auth.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Features.Authentication.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IAuthDbContext _context;

    public GetUsersQueryHandler(IAuthDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
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
                ProfilePicUrl = u.ProfilePicUrl,
                IsActived = u.IsActived,
                UserInternalId = u.UserInternalId
            })
            .ToListAsync(cancellationToken);

        return users;
    }
}
