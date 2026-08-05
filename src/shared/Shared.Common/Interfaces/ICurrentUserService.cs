using System;
using System.Collections.Generic;

namespace Shared.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Email { get; }
        IReadOnlyCollection<string> Roles { get; }
        string? RoleTeam { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
    }
}
