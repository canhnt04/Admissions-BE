using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Abstractions;

namespace Shared.Messaging.RabbitMQ;

public class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqEventBus(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
        InitRabbitMq();
    }

    private void InitRabbitMq()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: _options.ExchangeName, type: ExchangeType.Direct);
    }

    public void Publish(IIntegrationEvent @event)
    {
        var eventName = @event.GetType().Name;
        var message = JsonSerializer.Serialize(@event, @event.GetType());
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: _options.ExchangeName,
            routingKey: eventName,
            basicProperties: null,
            body: body);
    }

    public void Subscribe<T, TH>()
        where T : IIntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name;
        _channel.QueueDeclare(queue: eventName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: eventName, exchange: _options.ExchangeName, routingKey: eventName);
        // Basic consumer logic to be implemented...
    }

    public void Unsubscribe<T, TH>()
        where T : IIntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        // Unsubscribe logic...
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
