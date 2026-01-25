using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;

namespace POS.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    [HttpPost("stockin")]
    public async Task<IActionResult> StockIn(Guid productId, decimal qty, string remarks)
    {
        await _service.StockInAsync(productId, qty, "MANUAL", null, remarks);
        return Ok();
    }

    [HttpPost("stockout")]
    public async Task<IActionResult> StockOut(Guid productId, decimal qty, string remarks)
    {
        await _service.StockOutAsync(productId, qty, "MANUAL", null, remarks);
        return Ok();
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetStock(Guid productId)
        => Ok(await _service.GetStockAsync(productId));
}
