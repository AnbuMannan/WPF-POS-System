using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<List<SalesSummaryReportRow>>> GetSalesReport([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? customerId, [FromQuery] string? status)
        => Ok(await _service.GetSalesReportAsync(from, to, customerId, status));

    [HttpGet("item-wise-sales")]
    public async Task<ActionResult<List<ItemWiseSalesRow>>> GetItemWiseSales([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? categoryId)
        => Ok(await _service.GetItemWiseSalesAsync(from, to, categoryId));

    [HttpGet("profit-loss")]
    public async Task<ActionResult<ProfitLossReportDto>> GetProfitLoss([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _service.GetProfitLossReportAsync(from, to));

    [HttpGet("low-stock")]
    public async Task<ActionResult<List<LowStockItemRow>>> GetLowStock([FromQuery] decimal threshold = 0)
        => Ok(await _service.GetLowStockReportAsync(threshold));
}

