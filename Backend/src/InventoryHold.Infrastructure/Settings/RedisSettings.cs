namespace InventoryHold.Infrastructure.Settings;

public sealed class RedisSettings
{
    public string ConnectionString { get; init; } = "redis:6379";
    public string InventoryCacheKey { get; init; } = "inventory_snapshot";
    public int InventoryCacheTtlSeconds { get; init; } = 30;
}
