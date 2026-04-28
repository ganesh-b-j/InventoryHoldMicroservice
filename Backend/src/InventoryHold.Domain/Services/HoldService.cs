namespace InventoryHold.Domain.Services;

using InventoryHold.Contracts;
using InventoryHold.Domain.Caching;
using InventoryHold.Domain.Messaging;
using InventoryHold.Domain.Repositories;

public sealed class HoldService : IHoldService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IHoldRepository _holdRepository;
    private readonly IInventoryCache _inventoryCache;
    private readonly IEventPublisher _eventPublisher;
    private readonly int _defaultHoldMinutes = 15;

    public HoldService(
        IInventoryRepository inventoryRepository,
        IHoldRepository holdRepository,
        IInventoryCache inventoryCache,
        IEventPublisher eventPublisher)
    {
        _inventoryRepository = inventoryRepository;
        _holdRepository = holdRepository;
        _inventoryCache = inventoryCache;
        _eventPublisher = eventPublisher;
    }

    public async Task<HoldResponse> CreateHoldAsync(CreateHoldRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException("At least one item is required for a hold.", nameof(request.Items));
        }

        var requestedItems = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        if (requestedItems.Any(i => i.Quantity <= 0))
        {
            throw new ArgumentException("Quantities must be greater than zero.");
        }

        var success = await _inventoryRepository.ReserveItemsAsync(requestedItems, cancellationToken);
        if (!success)
        {
            throw new InvalidOperationException("Insufficient inventory to create the hold.");
        }

        var inventory = await _inventoryRepository.GetInventoryAsync(cancellationToken);
        var holdItems = request.Items.Select(item =>
        {
            var product = inventory.FirstOrDefault(i => i.ProductId == item.ProductId);
            return new HoldItem(item.ProductId, product?.ProductName ?? string.Empty, item.Quantity);
        }).ToList();

        var now = DateTime.UtcNow;
        var hold = new InventoryHoldAggregate(
            Guid.NewGuid().ToString("N"),
            request.CustomerId,
            holdItems,
            now,
            now.AddMinutes(request.DurationMinutes ?? _defaultHoldMinutes),
            HoldStatus.Active);

        var persisted = await _holdRepository.CreateHoldAsync(hold, cancellationToken);
        await _inventoryCache.InvalidateInventoryCacheAsync(cancellationToken);
        await _eventPublisher.PublishAsync("HoldCreated", new
        {
            HoldId = persisted.Id,
            persisted.CustomerId,
            Items = persisted.Items,
            persisted.CreatedAt,
            persisted.ExpiresAt
        }, cancellationToken);

        return ToResponse(persisted);
    }

    public async Task<HoldResponse> GetHoldAsync(string holdId, CancellationToken cancellationToken = default)
    {
        var hold = await _holdRepository.GetHoldAsync(holdId, cancellationToken);
        if (hold is null)
        {
            throw new KeyNotFoundException("Hold not found.");
        }

        if (hold.IsExpired(DateTime.UtcNow))
        {
            hold = hold with { Status = HoldStatus.Expired };
            await _holdRepository.UpdateHoldAsync(hold, cancellationToken);
            await _inventoryRepository.ReleaseItemsAsync(hold.Items.Select(i => (i.ProductId, i.Quantity)).ToList(), cancellationToken);
            await _inventoryCache.InvalidateInventoryCacheAsync(cancellationToken);
            await _eventPublisher.PublishAsync("HoldExpired", new
            {
                HoldId = hold.Id,
                hold.CustomerId,
                hold.Items,
                hold.ExpiresAt
            }, cancellationToken);
            throw new KeyNotFoundException("Hold not found.");
        }

        return ToResponse(hold);
    }

    public async Task ReleaseHoldAsync(string holdId, CancellationToken cancellationToken = default)
    {
        var hold = await _holdRepository.GetHoldAsync(holdId, cancellationToken);
        if (hold is null)
        {
            throw new KeyNotFoundException("Hold not found.");
        }

        if (hold.Status != HoldStatus.Active)
        {
            throw new InvalidOperationException("Only active holds can be released.");
        }

        if (hold.IsExpired(DateTime.UtcNow))
        {
            hold = hold with { Status = HoldStatus.Expired };
            await _holdRepository.UpdateHoldAsync(hold, cancellationToken);
            await _inventoryRepository.ReleaseItemsAsync(hold.Items.Select(i => (i.ProductId, i.Quantity)).ToList(), cancellationToken);
            await _inventoryCache.InvalidateInventoryCacheAsync(cancellationToken);
            await _eventPublisher.PublishAsync("HoldExpired", new
            {
                HoldId = hold.Id,
                hold.CustomerId,
                hold.Items,
                hold.ExpiresAt
            }, cancellationToken);
            throw new InvalidOperationException("Hold has already expired.");
        }

        var released = hold with { Status = HoldStatus.Released, ReleasedAt = DateTime.UtcNow };
        await _holdRepository.UpdateHoldAsync(released, cancellationToken);
        await _inventoryRepository.ReleaseItemsAsync(released.Items.Select(i => (i.ProductId, i.Quantity)).ToList(), cancellationToken);
        await _inventoryCache.InvalidateInventoryCacheAsync(cancellationToken);
        await _eventPublisher.PublishAsync("HoldReleased", new
        {
            HoldId = released.Id,
            released.CustomerId,
            released.Items,
            released.ReleasedAt
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryItemResponse>> GetInventoryAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _inventoryCache.GetInventoryAsync(cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var inventory = await _inventoryRepository.GetInventoryAsync(cancellationToken);
        var response = inventory.Select(i => new InventoryItemResponse(i.ProductId, i.ProductName, i.AvailableQuantity)).ToList();
        await _inventoryCache.SetInventoryAsync(response, TimeSpan.FromSeconds(30), cancellationToken);
        return response;
    }

    private static HoldResponse ToResponse(InventoryHoldAggregate hold)
    {
        return new HoldResponse(
            hold.Id,
            hold.CustomerId,
            hold.Items.Select(i => new HoldItemResponse(i.ProductId, i.ProductName, i.Quantity)).ToList(),
            hold.CreatedAt,
            hold.ExpiresAt,
            hold.Status.ToString());
    }
}
