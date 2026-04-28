using InventoryHold.Contracts;
using InventoryHold.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryHold.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InventoryController : ControllerBase
{
    private readonly IHoldService _holdService;

    public InventoryController(IHoldService holdService)
    {
        _holdService = holdService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(InventorySnapshotResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventory(CancellationToken cancellationToken)
    {
        var inventory = await _holdService.GetInventoryAsync(cancellationToken);
        return Ok(new InventorySnapshotResponse(inventory));
    }
}
