using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/loyalty")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyService _service;

    public LoyaltyController(ILoyaltyService service)
    {
        _service = service;
    }

    // GET: api/loyalty/settings
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _service.GetSettingsAsync();
        if (settings == null)
        {
            return Ok(new LoyaltySettingsDto
            {
                PointsPerUnitCurrency = 0,
                RedemptionValuePerPoint = 0,
                MinimumRedeemPoints = 0
            });
        }

        return Ok(settings);
    }

    // PUT: api/loyalty/settings
    [HttpPut("settings")]
    public async Task<IActionResult> SaveSettings([FromBody] UpdateLoyaltySettingsDto dto)
    {
        try
        {
            var settings = await _service.SaveSettingsAsync(dto);
            return Ok(settings);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/loyalty/calculate?amount=100
    [HttpGet("calculate")]
    public async Task<IActionResult> CalculatePoints([FromQuery] decimal amount)
    {
        var points = await _service.CalculatePointsAsync(amount);
        return Ok(new { points });
    }

    // POST: api/loyalty/redeem
    [HttpPost("redeem")]
    public async Task<IActionResult> RedeemPoints([FromBody] RedeemPointsRequest request)
    {
        try
        {
            var result = await _service.RedeemPointsAsync(request.CustomerId, request.PointsToRedeem);
            return Ok(result);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

