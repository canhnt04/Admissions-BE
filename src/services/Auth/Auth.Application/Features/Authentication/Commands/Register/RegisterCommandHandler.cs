using Auth.Application.Common.Helpers;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Domain.Errors;
using MediatR;
using Shared.Common.Exceptions;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthDbContext _context;
    private readonly IUserEventPublisher _eventPublisher;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IAuthDbContext context,
        IUserEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _context = context;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.IsUserNameTakenAsync(request.UserName, cancellationToken))
            return Result.Failure(AuthErrors.DuplicateUsername);

        PasswordHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            PasswordHash = Convert.ToBase64String(passwordHash),
            PasswordSalt = passwordSalt,
            FullName = request.FullName,
            Mobile = request.Mobile,
            IdentificationNumber = request.IdentificationNumber,
            Role = Role.User,
            ProfilePicUrl = "",
            IsActived = true,
            UserInternalId = $"EMP{new Random().Next(1000, 9999)}"
        };

        _userRepository.Add(user);

        // Publish UserSyncEvent — các CRM service sẽ tạo UserReplica tương ứng
        await _eventPublisher.PublishUserSyncAsync(
            user.Id,
            user.FullName,
            "", // Add Email if available or empty string
            user.Mobile,
            (int)user.Role,
            user.TeamId,
            user.IsActived,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
