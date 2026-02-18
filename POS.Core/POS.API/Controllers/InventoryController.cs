using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;

namespace POS.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;
    private readonly IItemLedgerService _ledgerService;

    public InventoryController(IInventoryService service, IItemLedgerService ledgerService)
    {
        _service = service;
        _ledgerService = ledgerService;
    }

    [HttpPost("stockin")]
    public async Task<IActionResult> StockIn(long productId, decimal qty, string remarks)
    {
        await _service.StockInAsync(productId, qty, "MANUAL", null, remarks);
        return Ok();
    }

    [HttpPost("stockout")]
    public async Task<IActionResult> StockOut(long productId, decimal qty, string remarks)
    {
        await _service.StockOutAsync(productId, qty, "MANUAL", null, remarks);
        return Ok();
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetStock(long productId)
        => Ok(await _service.GetStockAsync(productId));

    /// <summary>
    /// Get item ledger for a specific product with running balance
    /// </summary>
    [HttpGet("ledger/{productId}")]
    public async Task<IActionResult> GetLedger(
        long productId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _ledgerService.GetLedgerAsync(productId, fromDate, toDate);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get ledger entries only (without summary)
    /// </summary>
    [HttpGet("ledger/{productId}/entries")]
    public async Task<IActionResult> GetLedgerEntries(
        long productId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _ledgerService.GetEntriesAsync(productId, fromDate, toDate);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
