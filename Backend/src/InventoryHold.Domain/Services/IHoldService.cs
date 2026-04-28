namespace InventoryHold.Domain.Services;

using InventoryHold.Contracts;

public interface IHoldService
{
    Task<HoldResponse> CreateHoldAsync(CreateHoldRequest request, CancellationToken cancellationToken = default);
    Task<HoldResponse> GetHoldAsync(string holdId, CancellationToken cancellationToken = default);
    Task ReleaseHoldAsync(string holdId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItemResponse>> GetInventoryAsync(CancellationToken cancellationToken = default);
}
