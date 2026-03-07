using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;

namespace POS.API.Controllers;

[ApiController]
[Route("api/eod-reports")]
public class EODReportsController : ControllerBase
{
    private readonly IEODReportService _service;

    public EODReportsController(IEODReportService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime date, CancellationToken cancellationToken = default)
    {

        var report = await _service.GetEODReportAsync(date.Date, cancellationToken);
        return Ok(report);
    }

    /// <summary>Close day: mark all sales for the date as locked (prevent editing).</summary>
    [HttpPost("close-day")]
    public async Task<IActionResult> CloseDay([FromQuery] DateTime date, [FromQuery] string? lockedBy = null, CancellationToken cancellationToken = default)
    {

        await _service.CloseDayReportAsync(date.Date, lockedBy, cancellationToken);
        return Ok(new { closed = true, date = date.Date });
    }
}
