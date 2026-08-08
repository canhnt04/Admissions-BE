using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;

using Shared.Common;

namespace Auth.Application.Features.Authentication.Commands.Register;

public class RegisterCommand : IRequest<Result>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
}
