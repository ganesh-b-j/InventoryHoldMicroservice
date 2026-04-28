namespace InventoryHold.Domain.Repositories;

public interface IInventoryRepository
{
    Task<IReadOnlyList<ProductInventory>> GetInventoryAsync(CancellationToken cancellationToken = default);
    Task<ProductInventory?> GetProductAsync(string productId, CancellationToken cancellationToken = default);
    Task<bool> ReserveItemsAsync(IReadOnlyList<(string ProductId, int Quantity)> items, CancellationToken cancellationToken = default);
    Task ReleaseItemsAsync(IReadOnlyList<(string ProductId, int Quantity)> items, CancellationToken cancellationToken = default);
    Task SeedProductsAsync(CancellationToken cancellationToken = default);
}
