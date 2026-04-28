namespace InventoryHold.Infrastructure.Services;

using InventoryHold.Domain;
using InventoryHold.Domain.Caching;
using InventoryHold.Domain.Messaging;
using InventoryHold.Domain.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class ExpiredHoldBackgroundService : BackgroundService
{
    private readonly IHoldRepository _holdRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryCache _cache;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ExpiredHoldBackgroundService> _logger;

    public ExpiredHoldBackgroundService(
        IHoldRepository holdRepository,
        IInventoryRepository inventoryRepository,
        IInventoryCache cache,
        IEventPublisher eventPublisher,
        ILogger<ExpiredHoldBackgroundService> logger)
    {
        _holdRepository = holdRepository;
        _inventoryRepository = inventoryRepository;
        _cache = cache;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var expiredHolds = await _holdRepository.GetExpiredHoldsAsync(now, stoppingToken);
                foreach (var hold in expiredHolds)
                {
                    _logger.LogInformation("Expiring hold {HoldId}", hold.Id);
                    var expired = hold with { Status = HoldStatus.Expired };
                    await _holdRepository.UpdateHoldAsync(expired, stoppingToken);
                    await _inventoryRepository.ReleaseItemsAsync(expired.Items.Select(i => (i.ProductId, i.Quantity)).ToList(), stoppingToken);
                    await _cache.InvalidateInventoryCacheAsync(stoppingToken);
                    await _eventPublisher.PublishAsync("HoldExpired", new
                    {
                        HoldId = expired.Id,
                        expired.CustomerId,
                        expired.Items,
                        expired.ExpiresAt
                    }, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing expired holds.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
