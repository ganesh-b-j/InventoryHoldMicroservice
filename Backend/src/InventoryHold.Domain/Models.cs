namespace InventoryHold.Domain;

public enum HoldStatus
{
    Active,
    Released,
    Expired
}

public sealed record HoldItem(string ProductId, string ProductName, int Quantity);

public sealed record InventoryHoldAggregate(
    string Id,
    string CustomerId,
    IReadOnlyList<HoldItem> Items,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    HoldStatus Status,
    DateTime? ReleasedAt = null)
{
    public bool IsExpired(DateTime now) => Status == HoldStatus.Active && ExpiresAt <= now;
}

public sealed record ProductInventory(string ProductId, string ProductName, int AvailableQuantity);
