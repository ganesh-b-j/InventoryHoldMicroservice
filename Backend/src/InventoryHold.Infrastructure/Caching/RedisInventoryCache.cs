namespace InventoryHold.Infrastructure.Caching;

using System.Text.Json;
using InventoryHold.Contracts;
using InventoryHold.Domain.Caching;
using InventoryHold.Infrastructure.Settings;
using StackExchange.Redis;

public sealed class RedisInventoryCache : IInventoryCache
{
    private readonly IDatabase _database;
    private readonly RedisSettings _settings;

    public RedisInventoryCache(RedisSettings settings)
    {
        _settings = settings;
        var connection = ConnectionMultiplexer.Connect(settings.ConnectionString);
        _database = connection.GetDatabase();
    }

    public async Task<IReadOnlyList<InventoryItemResponse>?> GetInventoryAsync(CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(_settings.InventoryCacheKey);
        if (!value.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<IReadOnlyList<InventoryItemResponse>>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task SetInventoryAsync(IReadOnlyList<InventoryItemResponse> inventory, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        await _database.StringSetAsync(_settings.InventoryCacheKey, JsonSerializer.Serialize(inventory), ttl);
    }

    public Task InvalidateInventoryCacheAsync(CancellationToken cancellationToken = default)
    {
        return _database.KeyDeleteAsync(_settings.InventoryCacheKey);
    }
}
