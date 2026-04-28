namespace InventoryHold.Infrastructure.Repositories;

using InventoryHold.Domain;
using InventoryHold.Domain.Repositories;
using InventoryHold.Infrastructure.Models;
using InventoryHold.Infrastructure.Settings;
using MongoDB.Driver;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly IMongoCollection<ProductDocument> _products;

    public InventoryRepository(MongoSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _products = database.GetCollection<ProductDocument>("products");
        var index = Builders<ProductDocument>.IndexKeys.Ascending(p => p.ProductId);
        _products.Indexes.CreateOne(new CreateIndexModel<ProductDocument>(index, new CreateIndexOptions { Unique = true }));
    }

    public async Task<IReadOnlyList<ProductInventory>> GetInventoryAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _products.Find(FilterDefinition<ProductDocument>.Empty).ToListAsync(cancellationToken);
        return documents.Select(d => new ProductInventory(d.ProductId, d.ProductName, d.AvailableQuantity)).ToList();
    }

    public async Task<ProductInventory?> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        var document = await _products.Find(p => p.ProductId == productId).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : new ProductInventory(document.ProductId, document.ProductName, document.AvailableQuantity);
    }

    public async Task<bool> ReserveItemsAsync(IReadOnlyList<(string ProductId, int Quantity)> items, CancellationToken cancellationToken = default)
    {
        var reserved = new List<(string ProductId, int Quantity)>();
        foreach (var item in items)
        {
            var updateResult = await _products.UpdateOneAsync(
                p => p.ProductId == item.ProductId && p.AvailableQuantity >= item.Quantity,
                Builders<ProductDocument>.Update.Inc(p => p.AvailableQuantity, -item.Quantity),
                cancellationToken: cancellationToken);

            if (updateResult.ModifiedCount == 0)
            {
                if (reserved.Count > 0)
                {
                    await ReleaseItemsAsync(reserved, cancellationToken);
                }
                return false;
            }
            reserved.Add(item);
        }

        return true;
    }

    public async Task ReleaseItemsAsync(IReadOnlyList<(string ProductId, int Quantity)> items, CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
        {
            await _products.UpdateOneAsync(
                p => p.ProductId == item.ProductId,
                Builders<ProductDocument>.Update.Inc(p => p.AvailableQuantity, item.Quantity),
                cancellationToken: cancellationToken);
        }
    }

    public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
    {
        var count = await _products.CountDocumentsAsync(FilterDefinition<ProductDocument>.Empty, cancellationToken: cancellationToken);
        if (count > 0)
        {
            return;
        }

        var products = new[]
        {
            new ProductDocument { ProductId = "sku-001", ProductName = "Rustic Notebook", AvailableQuantity = 28 },
            new ProductDocument { ProductId = "sku-002", ProductName = "Travel Mug", AvailableQuantity = 15 },
            new ProductDocument { ProductId = "sku-003", ProductName = "Desk Lamp", AvailableQuantity = 12 },
            new ProductDocument { ProductId = "sku-004", ProductName = "Wireless Charger", AvailableQuantity = 21 },
            new ProductDocument { ProductId = "sku-005", ProductName = "Canvas Tote", AvailableQuantity = 34 }
        };

        await _products.InsertManyAsync(products, cancellationToken: cancellationToken);
    }
}
