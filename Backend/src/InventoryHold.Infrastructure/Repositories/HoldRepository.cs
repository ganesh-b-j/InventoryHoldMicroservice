namespace InventoryHold.Infrastructure.Repositories;

using InventoryHold.Domain;
using InventoryHold.Domain.Repositories;
using InventoryHold.Infrastructure.Models;
using InventoryHold.Infrastructure.Settings;
using MongoDB.Driver;

public sealed class HoldRepository : IHoldRepository
{
    private readonly IMongoCollection<HoldDocument> _holds;

    public HoldRepository(MongoSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _holds = database.GetCollection<HoldDocument>("holds");
        var index = Builders<HoldDocument>.IndexKeys.Ascending(h => h.HoldId);
        _holds.Indexes.CreateOne(new CreateIndexModel<HoldDocument>(index, new CreateIndexOptions { Unique = true }));
    }

    public async Task<InventoryHoldAggregate> CreateHoldAsync(InventoryHoldAggregate hold, CancellationToken cancellationToken = default)
    {
        var document = ToDocument(hold);
        await _holds.InsertOneAsync(document, cancellationToken: cancellationToken);
        return hold;
    }

    public async Task<InventoryHoldAggregate?> GetHoldAsync(string holdId, CancellationToken cancellationToken = default)
    {
        var document = await _holds.Find(h => h.HoldId == holdId).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ToAggregate(document);
    }

    public async Task<InventoryHoldAggregate?> UpdateHoldAsync(InventoryHoldAggregate hold, CancellationToken cancellationToken = default)
    {
        var existing = await _holds.Find(h => h.HoldId == hold.Id).FirstOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var document = ToDocument(hold, existing.Id);
        var replaced = await _holds.FindOneAndReplaceAsync(h => h.HoldId == hold.Id, document, cancellationToken: cancellationToken);
        return replaced is null ? null : hold;
    }

    public async Task<IReadOnlyList<InventoryHoldAggregate>> GetExpiredHoldsAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        var documents = await _holds.Find(h => h.Status == HoldStatus.Active && h.ExpiresAt <= now).ToListAsync(cancellationToken);
        return documents.Select(ToAggregate).ToList();
    }

    private static HoldDocument ToDocument(InventoryHoldAggregate hold, string? id = null)
    {
        return new HoldDocument
        {
            Id = id,
            HoldId = hold.Id,
            CustomerId = hold.CustomerId,
            Items = hold.Items.ToList(),
            CreatedAt = hold.CreatedAt,
            ExpiresAt = hold.ExpiresAt,
            Status = hold.Status,
            ReleasedAt = hold.ReleasedAt
        };
    }

    private static InventoryHoldAggregate ToAggregate(HoldDocument document)
    {
        return new InventoryHoldAggregate(
            document.HoldId,
            document.CustomerId,
            document.Items,
            document.CreatedAt,
            document.ExpiresAt,
            document.Status,
            document.ReleasedAt);
    }
}
