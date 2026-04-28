namespace InventoryHold.Contracts;

public sealed record HoldItemRequest(string ProductId, int Quantity);
public sealed record CreateHoldRequest(string CustomerId, List<HoldItemRequest> Items, int? DurationMinutes = null);
public sealed record HoldItemResponse(string ProductId, string ProductName, int Quantity);
public sealed record HoldResponse(string HoldId, string CustomerId, IReadOnlyList<HoldItemResponse> Items, DateTime CreatedAt, DateTime ExpiresAt, string Status);
public sealed record InventoryItemResponse(string ProductId, string ProductName, int AvailableQuantity);
public sealed record ErrorResponse(string Code, string Message);
public sealed record InventorySnapshotResponse(IReadOnlyList<InventoryItemResponse> Items);
