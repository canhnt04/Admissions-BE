using System;

namespace Shared.Contracts.Events.Auth
{
    public class UserSyncEvent
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public int Role { get; set; }
        public Guid? TeamId { get; set; }
        public bool IsActive { get; set; }
    }
}
