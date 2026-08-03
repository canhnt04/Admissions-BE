using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;

namespace Auth.Application.Features.Authentication.Commands.Register;

public class RegisterCommand : IRequest<RegisterResponse>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
}

public class RegisterResponse
{
    public string Message { get; set; } = string.Empty;
}
