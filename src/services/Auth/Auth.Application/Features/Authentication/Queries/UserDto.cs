using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using Auth.Domain.Entities;

namespace Auth.Application.Features.Authentication.Queries;

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public Role Role { get; set; }
    public Guid? TeamId { get; set; }
    public string ProfilePicUrl { get; set; } = string.Empty;
    public bool IsActived { get; set; }
    public string UserInternalId { get; set; } = string.Empty;
}
