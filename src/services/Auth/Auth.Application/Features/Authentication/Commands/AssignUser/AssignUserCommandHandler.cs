using Auth.Application.Common.Interfaces;
using Auth.Domain.Errors;
using MediatR;
using Shared.Common.Exceptions;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Commands.AssignUser;

public class AssignUserCommandHandler : IRequestHandler<AssignUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IAuthDbContext _context;
    private readonly IUserEventPublisher _eventPublisher;

    public AssignUserCommandHandler(
        IUserRepository userRepository,
        ITeamRepository teamRepository,
        IAuthDbContext context,
        IUserEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _teamRepository = teamRepository;
        _context = context;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result> Handle(AssignUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return Result.Failure(AuthErrors.UserNotFound);

        if (request.Role.HasValue)
        {
            user.Role = request.Role.Value;
        }

        if (request.TeamId.HasValue)
        {
            var team = await _teamRepository.GetByIdAsync(request.TeamId.Value, cancellationToken);
            if (team == null) return Result.Failure(AuthErrors.TeamNotFound);
            
            user.TeamId = request.TeamId;
        }

        _userRepository.Update(user);

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

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
