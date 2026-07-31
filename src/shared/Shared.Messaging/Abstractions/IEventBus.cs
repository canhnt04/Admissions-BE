namespace Shared.Messaging.Abstractions;

public interface IEventBus
{
    void Publish(IIntegrationEvent @event);
    void Subscribe<T, TH>()
        where T : IIntegrationEvent
        where TH : IIntegrationEventHandler<T>;
    void Unsubscribe<T, TH>()
        where T : IIntegrationEvent
        where TH : IIntegrationEventHandler<T>;
}
