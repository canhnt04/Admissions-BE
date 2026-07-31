using Auth.Domain.Errors;
using Auth.Application.Common.Interfaces;
using MediatR;
using Shared.Common.Exceptions;

namespace Auth.Application.Features.Authentication.Commands.AssignUser;

public class AssignUserCommandHandler : IRequestHandler<AssignUserCommand, AssignUserResponse>
{
    private readonly IAuthDbContext _context;
    private readonly IUserEventPublisher _eventPublisher;

    public AssignUserCommandHandler(IAuthDbContext context, IUserEventPublisher eventPublisher)
    {
        _context = context;
        _eventPublisher = eventPublisher;
    }

    public async Task<AssignUserResponse> Handle(AssignUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null) throw new NotFoundException(AuthErrors.UserNotFound);

        if (request.TeamId.HasValue)
        {
            var team = await _context.Teams.FindAsync(new object[] { request.TeamId.Value }, cancellationToken);
            if (team == null) throw new NotFoundException(AuthErrors.TeamNotFound);
        }

        user.Role = request.Role;
        user.TeamId = request.TeamId;

        await _context.SaveChangesAsync(cancellationToken);

        // Publish sync event via abstract publisher
        await _eventPublisher.PublishUserSyncAsync(
            user.Id,
            user.FullName,
            "",
            user.Mobile,
            (int)user.Role,
            user.TeamId,
            user.IsActived,
            cancellationToken);

        return new AssignUserResponse
        {
            Message = "User role and team assigned successfully."
        };
    }
}
