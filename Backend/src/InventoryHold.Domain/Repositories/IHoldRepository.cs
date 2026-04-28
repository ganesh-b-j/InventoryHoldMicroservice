namespace InventoryHold.Domain.Repositories;

public interface IHoldRepository
{
    Task<InventoryHoldAggregate> CreateHoldAsync(InventoryHoldAggregate hold, CancellationToken cancellationToken = default);
    Task<InventoryHoldAggregate?> GetHoldAsync(string holdId, CancellationToken cancellationToken = default);
    Task<InventoryHoldAggregate?> UpdateHoldAsync(InventoryHoldAggregate hold, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryHoldAggregate>> GetExpiredHoldsAsync(DateTime now, CancellationToken cancellationToken = default);
}
