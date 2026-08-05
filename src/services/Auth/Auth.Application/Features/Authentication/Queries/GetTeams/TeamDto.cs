using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using System;

namespace Auth.Application.Features.Authentication.Queries.GetTeams;

public class TeamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public RoleTeam? RoleTeam { get; set; }
    public bool IsActive { get; set; }
}
