namespace InventoryHold.Infrastructure.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public sealed class ProductDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; init; }
    public string ProductId { get; init; } = default!;
    public string ProductName { get; init; } = default!;
    public int AvailableQuantity { get; init; }
}
