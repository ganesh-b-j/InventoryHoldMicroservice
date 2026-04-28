namespace InventoryHold.Infrastructure.Messaging;

using System.Text;
using System.Text.Json;
using InventoryHold.Domain.Messaging;
using InventoryHold.Infrastructure.Settings;
using RabbitMQ.Client;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _connectionLock = new();

    public RabbitMqEventPublisher(RabbitMqSettings settings)
    {
        _settings = settings;
    }

    public Task PublishAsync(string eventType, object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureConnection();

            var envelope = JsonSerializer.Serialize(new { EventType = eventType, Payload = payload });
            var body = Encoding.UTF8.GetBytes(envelope);
            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(_settings.ExchangeName, string.Empty, properties, body);
        }
        catch
        {
            // Do not fail the entire request pipeline if RabbitMQ is unavailable.
        }

        return Task.CompletedTask;
    }

    private void EnsureConnection()
    {
        if (_channel is { IsOpen: true })
        {
            return;
        }

        lock (_connectionLock)
        {
            if (_channel is { IsOpen: true })
            {
                return;
            }

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                DispatchConsumersAsync = true
            };

            _connection?.Dispose();
            _channel?.Dispose();

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
