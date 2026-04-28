namespace InventoryHold.Infrastructure;

using InventoryHold.Domain.Caching;
using InventoryHold.Domain.Messaging;
using InventoryHold.Domain.Repositories;
using InventoryHold.Domain.Services;
using InventoryHold.Infrastructure.Caching;
using InventoryHold.Infrastructure.Messaging;
using InventoryHold.Infrastructure.Repositories;
using InventoryHold.Infrastructure.Services;
using InventoryHold.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class StartupExtensions
{
    public static IServiceCollection AddInventoryHoldInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoSettings>(configuration.GetSection("Mongo"));
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));

        var mongoSettings = configuration.GetSection("Mongo").Get<MongoSettings>()!;
        var redisSettings = configuration.GetSection("Redis").Get<RedisSettings>()!;
        var rabbitMqSettings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()!;

        services.AddSingleton(mongoSettings);
        services.AddSingleton(redisSettings);
        services.AddSingleton(rabbitMqSettings);

        services.AddSingleton<IInventoryRepository, InventoryRepository>();
        services.AddSingleton<IHoldRepository, HoldRepository>();
        services.AddSingleton<IInventoryCache, RedisInventoryCache>();
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddSingleton<IHoldService, HoldService>();
        services.AddHostedService<ExpiredHoldBackgroundService>();

        return services;
    }
}
