namespace InventoryHold.Infrastructure.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using InventoryHold.Domain;

public sealed class HoldDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; init; }
    public string HoldId { get; init; } = default!;
    public string CustomerId { get; init; } = default!;
    public List<HoldItem> Items { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public HoldStatus Status { get; init; }
    public DateTime? ReleasedAt { get; init; }
}
