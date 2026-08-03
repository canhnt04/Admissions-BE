using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;

namespace Auth.Application.Features.Authentication.Commands.Login;

public class LoginCommand : IRequest<LoginResponse>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
