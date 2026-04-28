namespace InventoryHold.Domain.Caching;

using InventoryHold.Contracts;

public interface IInventoryCache
{
    Task<IReadOnlyList<InventoryItemResponse>?> GetInventoryAsync(CancellationToken cancellationToken = default);
    Task SetInventoryAsync(IReadOnlyList<InventoryItemResponse> inventory, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task InvalidateInventoryCacheAsync(CancellationToken cancellationToken = default);
}
