using Auth.Domain.Enums;
using Shared.Contracts.Enums;
namespace Auth.Application.Features.Authentication.Commands.Login;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
}
