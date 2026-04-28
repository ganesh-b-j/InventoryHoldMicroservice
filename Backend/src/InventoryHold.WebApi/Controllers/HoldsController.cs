using InventoryHold.Contracts;
using InventoryHold.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryHold.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HoldsController : ControllerBase
{
    private readonly IHoldService _holdService;

    public HoldsController(IHoldService holdService)
    {
        _holdService = holdService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(HoldResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateHold(CreateHoldRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var hold = await _holdService.CreateHoldAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetHold), new { holdId = hold.HoldId }, hold);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse("invalid_request", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse("insufficient_inventory", ex.Message));
        }
    }

    [HttpGet("{holdId}")]
    [ProducesResponseType(typeof(HoldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHold(string holdId, CancellationToken cancellationToken)
    {
        try
        {
            var hold = await _holdService.GetHoldAsync(holdId, cancellationToken);
            return Ok(hold);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse("hold_not_found", ex.Message));
        }
    }

    [HttpDelete("{holdId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReleaseHold(string holdId, CancellationToken cancellationToken)
    {
        try
        {
            await _holdService.ReleaseHoldAsync(holdId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse("hold_not_found", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse("invalid_hold_state", ex.Message));
        }
    }
}
