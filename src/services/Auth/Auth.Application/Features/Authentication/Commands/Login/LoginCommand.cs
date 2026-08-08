using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Commands.Login;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
