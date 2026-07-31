using Shared.Common;

namespace ShortTerm.Domain.Entities
{
    public class UserReplica : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public int Role { get; set; }
        public Guid? TeamId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
