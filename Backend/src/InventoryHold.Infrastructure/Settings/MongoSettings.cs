namespace InventoryHold.Infrastructure.Settings;

public sealed class MongoSettings
{
    public string ConnectionString { get; init; } = "mongodb://mongo:27017";
    public string DatabaseName { get; init; } = "InventoryHold";
}
