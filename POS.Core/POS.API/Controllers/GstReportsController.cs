using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;

[ApiController]
[Route("api/reports/gst")]
public class GstReportsController : ControllerBase
{
    private readonly IGstReportService _service;

    public GstReportsController(IGstReportService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(DateTime from, DateTime to)
        => Ok(await _service.GetSummaryAsync(from, to));

    [HttpGet("hsn")]
    public async Task<IActionResult> HsnSummary(DateTime from, DateTime to)
        => Ok(await _service.GetHsnSummaryAsync(from, to));

    [HttpGet("daily")]
    public async Task<IActionResult> Daily(DateTime from, DateTime to)
        => Ok(await _service.GetDailyCollectionAsync(from, to));
}
