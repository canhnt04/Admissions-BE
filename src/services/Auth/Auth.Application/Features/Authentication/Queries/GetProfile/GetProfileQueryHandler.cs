using Auth.Domain.Errors;
using Auth.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;

namespace Auth.Application.Features.Authentication.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserDto>
{
    private readonly IAuthDbContext _context;

    public GetProfileQueryHandler(IAuthDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
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
                ProfilePicUrl = u.ProfilePicUrl,
                IsActived = u.IsActived,
                UserInternalId = u.UserInternalId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null) throw new NotFoundException(AuthErrors.UserNotFound);

        return user;
    }
}
