using Auth.Application.Common.Interfaces;
using Auth.Domain.Errors;
using MediatR;
using Shared.Common.Exceptions;

namespace Auth.Application.Features.Authentication.Commands.RemoveUserTeam;

public class RemoveUserTeamCommandHandler : IRequestHandler<RemoveUserTeamCommand, RemoveUserTeamResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthDbContext _context;
    private readonly IUserEventPublisher _eventPublisher;

    public RemoveUserTeamCommandHandler(
        IUserRepository userRepository,
        IAuthDbContext context,
        IUserEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _context = context;
        _eventPublisher = eventPublisher;
    }

    public async Task<RemoveUserTeamResponse> Handle(RemoveUserTeamCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) throw new NotFoundException(AuthErrors.UserNotFound);

        user.TeamId = null;

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

        return new RemoveUserTeamResponse
        {
            Message = "Đã gỡ đội nhóm thành công."
        };
    }
}
