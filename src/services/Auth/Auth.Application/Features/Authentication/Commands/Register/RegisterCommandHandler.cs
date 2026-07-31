using Auth.Application.Common.Helpers;
using Auth.Domain.Entities;
using Auth.Domain.Errors;
using Auth.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;

namespace Auth.Application.Features.Authentication.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IAuthDbContext _context;
    private readonly IUserEventPublisher _eventPublisher;

    public RegisterCommandHandler(IAuthDbContext context, IUserEventPublisher eventPublisher)
    {
        _context = context;
        _eventPublisher = eventPublisher;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.UserName == request.UserName, cancellationToken))
            throw new ConflictException(AuthErrors.DuplicateUsername);

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

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

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

        return new RegisterResponse
        {
            Message = "Registration successful"
        };
    }
}
