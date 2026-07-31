namespace Shared.Common;

public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public abstract class BaseAuditableEntity : BaseAuditableEntity<Guid>
{
}
