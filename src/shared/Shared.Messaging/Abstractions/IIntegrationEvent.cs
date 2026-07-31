namespace Shared.Messaging.Abstractions;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime CreationDate { get; }
}
