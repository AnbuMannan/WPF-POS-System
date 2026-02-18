using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;

namespace POS.API.Controllers;

[ApiController]
[Route("api/batches")]
public class BatchesController : ControllerBase
{
    private readonly IBatchService _service;

    public BatchesController(IBatchService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var batch = await _service.GetByIdAsync(id);
        if (batch == null)
            return NotFound();
        return Ok(batch);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(long productId)
        => Ok(await _service.GetByProductIdAsync(productId));

    [HttpGet("product/{productId}/available")]
    public async Task<IActionResult> GetAvailableBatches(long productId)
        => Ok(await _service.GetAvailableBatchesAsync(productId));

    [HttpGet("expired")]
    public async Task<IActionResult> GetExpired()
        => Ok(await _service.GetExpiredBatchesAsync());

    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring([FromQuery] int days = 30)
        => Ok(await _service.GetExpiringBatchesAsync(days));

    [HttpGet("product/{productId}/stock/total")]
    public async Task<IActionResult> GetTotalStock(long productId)
        => Ok(await _service.GetTotalStockForProductAsync(productId));

    [HttpGet("product/{productId}/stock/available")]
    public async Task<IActionResult> GetAvailableStock(long productId)
        => Ok(await _service.GetAvailableStockForProductAsync(productId));
}
