using System;

namespace LeadAssignment.Domain.Entities
{
    /// <summary>
    /// Bản sao tối giản của User — lưu trong CRM databases
    /// </summary>
    public class UserReplica
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public Role Role { get; set; }
        public Guid? TeamId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
    }
}
