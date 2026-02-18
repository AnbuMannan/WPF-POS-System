using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;

namespace POS.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken, [FromQuery] DateTime? date = null)
    {
        var targetDate = date?.Date ?? DateTime.Today;
        var summary = await _service.GetSummaryAsync(targetDate, cancellationToken);
        return Ok(summary);
    }
}

