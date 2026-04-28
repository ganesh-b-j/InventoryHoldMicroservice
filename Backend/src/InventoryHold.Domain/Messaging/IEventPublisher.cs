namespace InventoryHold.Domain.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(string eventType, object payload, CancellationToken cancellationToken = default);
}
