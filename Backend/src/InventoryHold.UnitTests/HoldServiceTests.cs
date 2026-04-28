using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryHold.Contracts;
using InventoryHold.Domain;
using InventoryHold.Domain.Caching;
using InventoryHold.Domain.Messaging;
using InventoryHold.Domain.Repositories;
using InventoryHold.Domain.Services;
using Moq;
using NUnit.Framework;

namespace InventoryHold.UnitTests;

public class HoldServiceTests
{
    private Mock<IInventoryRepository> _inventoryRepository = null!;
    private Mock<IHoldRepository> _holdRepository = null!;
    private Mock<IInventoryCache> _inventoryCache = null!;
    private Mock<IEventPublisher> _eventPublisher = null!;
    private HoldService _service = null!;

    [SetUp]
    public void Setup()
    {
        _inventoryRepository = new Mock<IInventoryRepository>(MockBehavior.Strict);
        _holdRepository = new Mock<IHoldRepository>(MockBehavior.Strict);
        _inventoryCache = new Mock<IInventoryCache>(MockBehavior.Strict);
        _eventPublisher = new Mock<IEventPublisher>(MockBehavior.Strict);
        _service = new HoldService(_inventoryRepository.Object, _holdRepository.Object, _inventoryCache.Object, _eventPublisher.Object);
    }

    [Test]
    public async Task CreateHoldAsync_ReturnsHold_WhenInventoryIsReserved()
    {
        var request = new CreateHoldRequest("customer-1", new List<HoldItemRequest> { new("sku-001", 2) }, 15);
        _inventoryRepository.Setup(x => x.ReserveItemsAsync(It.IsAny<IReadOnlyList<(string ProductId, int Quantity)>>(), default))
            .ReturnsAsync(true);
        _inventoryRepository.Setup(x => x.GetInventoryAsync(default))
            .ReturnsAsync(new List<ProductInventory> { new("sku-001", "Item", 8) });
        _holdRepository.Setup(x => x.CreateHoldAsync(It.IsAny<InventoryHoldAggregate>(), default))
            .ReturnsAsync((InventoryHoldAggregate hold, CancellationToken _) => hold);
        _inventoryCache.Setup(x => x.InvalidateInventoryCacheAsync(default)).Returns(Task.CompletedTask);
        _eventPublisher.Setup(x => x.PublishAsync("HoldCreated", It.IsAny<object>(), default)).Returns(Task.CompletedTask);

        var result = await _service.CreateHoldAsync(request);

        Assert.That(result.CustomerId, Is.EqualTo("customer-1"));
        Assert.That(result.Items.Single().ProductId, Is.EqualTo("sku-001"));
        _inventoryRepository.VerifyAll();
        _holdRepository.VerifyAll();
        _inventoryCache.VerifyAll();
        _eventPublisher.VerifyAll();
    }

    [Test]
    public void CreateHoldAsync_ThrowsInvalidOperationException_WhenInventoryIsInsufficient()
    {
        var request = new CreateHoldRequest("customer-1", new List<HoldItemRequest> { new("sku-001", 10) }, 15);
        _inventoryRepository.Setup(x => x.ReserveItemsAsync(It.IsAny<IReadOnlyList<(string ProductId, int Quantity)>>(), default))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateHoldAsync(request));
        _inventoryRepository.Verify(x => x.ReserveItemsAsync(It.IsAny<IReadOnlyList<(string ProductId, int Quantity)>>(), default), Times.Once);
    }

    [Test]
    public async Task GetHoldAsync_ExpiresHold_WhenHoldHasPassedExpiration()
    {
        var existing = new InventoryHoldAggregate(
            "hold-1",
            "customer-1",
            new List<HoldItem> { new("sku-001", "Item", 1) },
            DateTime.UtcNow.AddMinutes(-20),
            DateTime.UtcNow.AddMinutes(-5),
            HoldStatus.Active);

        _holdRepository.Setup(x => x.GetHoldAsync("hold-1", default)).ReturnsAsync(existing);
        _holdRepository.Setup(x => x.UpdateHoldAsync(It.Is<InventoryHoldAggregate>(h => h.Status == HoldStatus.Expired), default))
            .ReturnsAsync((InventoryHoldAggregate hold, CancellationToken _) => hold);
        _inventoryRepository.Setup(x => x.ReleaseItemsAsync(It.IsAny<IReadOnlyList<(string ProductId, int Quantity)>>(), default))
            .Returns(Task.CompletedTask);
        _inventoryCache.Setup(x => x.InvalidateInventoryCacheAsync(default)).Returns(Task.CompletedTask);
        _eventPublisher.Setup(x => x.PublishAsync("HoldExpired", It.IsAny<object>(), default)).Returns(Task.CompletedTask);

        Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetHoldAsync("hold-1"));
    }

    [Test]
    public async Task ReleaseHoldAsync_PublishesHoldReleased_WhenActiveHoldIsReleased()
    {
        var hold = new InventoryHoldAggregate(
            "hold-2",
            "customer-2",
            new List<HoldItem> { new("sku-001", "Item", 2) },
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(15),
            HoldStatus.Active);

        _holdRepository.Setup(x => x.GetHoldAsync("hold-2", default)).ReturnsAsync(hold);
        _holdRepository.Setup(x => x.UpdateHoldAsync(It.Is<InventoryHoldAggregate>(h => h.Status == HoldStatus.Released), default))
            .ReturnsAsync((InventoryHoldAggregate h, CancellationToken _) => h);
        _inventoryRepository.Setup(x => x.ReleaseItemsAsync(It.IsAny<IReadOnlyList<(string ProductId, int Quantity)>>(), default))
            .Returns(Task.CompletedTask);
        _inventoryCache.Setup(x => x.InvalidateInventoryCacheAsync(default)).Returns(Task.CompletedTask);
        _eventPublisher.Setup(x => x.PublishAsync("HoldReleased", It.IsAny<object>(), default)).Returns(Task.CompletedTask);

        await _service.ReleaseHoldAsync("hold-2");
        _holdRepository.VerifyAll();
        _eventPublisher.Verify(x => x.PublishAsync("HoldReleased", It.IsAny<object>(), default), Times.Once);
    }

    [Test]
    public async Task GetInventoryAsync_ReturnsCachedValue_WhenCacheExists()
    {
        var cached = new List<InventoryItemResponse> { new("sku-001", "Item", 5) };
        _inventoryCache.Setup(x => x.GetInventoryAsync(default)).ReturnsAsync(cached);

        var result = await _service.GetInventoryAsync();

        Assert.That(result, Is.EqualTo(cached));
        _inventoryCache.Verify(x => x.GetInventoryAsync(default), Times.Once);
    }
}

