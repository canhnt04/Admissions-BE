namespace Shared.Contracts;

public interface IEntity<TId>
{
    TId Id { get; set; }
}

public interface IEntity : IEntity<Guid>
{
}
