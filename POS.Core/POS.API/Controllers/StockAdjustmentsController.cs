using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/stock-adjustments")]
public class StockAdjustmentsController : ControllerBase
{
    private readonly IStockAdjustmentService _service;

    public StockAdjustmentsController(IStockAdjustmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var adjustment = await _service.GetByIdAsync(id);
        if (adjustment == null)
            return NotFound();
        return Ok(adjustment);
    }

    [HttpGet("reference/{referenceNo}")]
    public async Task<IActionResult> GetByReferenceNo(string referenceNo)
    {
        var adjustment = await _service.GetByReferenceNoAsync(referenceNo);
        if (adjustment == null)
            return NotFound();
        return Ok(adjustment);
    }

    [HttpGet("by-date-range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        => Ok(await _service.GetByDateRangeAsync(fromDate, toDate));

    [HttpGet("by-reason/{reason}")]
    public async Task<IActionResult> GetByReason(string reason)
        => Ok(await _service.GetByReasonAsync(reason));

    [HttpGet("by-status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
        => Ok(await _service.GetByStatusAsync(status));

    [HttpGet("reasons")]
    public IActionResult GetReasons()
        => Ok(AdjustmentReasons.All);

    /// <summary>
    /// Create and immediately approve a stock adjustment
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockAdjustmentDto dto, [FromQuery] string approvedBy = "System")
    {
        try
        {
            var result = await _service.CreateAndApproveAsync(dto, approvedBy);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        catch (ValidationException vex)
        {
            return BadRequest(new
            {
                errors = new Dictionary<string, string[]>
                {
                    { vex.Field, new[] { vex.Message } }
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a draft adjustment (not immediately processed)
    /// </summary>
    [HttpPost("draft")]
    public async Task<IActionResult> CreateDraft([FromBody] CreateStockAdjustmentDto dto)
    {
        try
        {
            var result = await _service.CreateDraftAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        catch (ValidationException vex)
        {
            return BadRequest(new
            {
                errors = new Dictionary<string, string[]>
                {
                    { vex.Field, new[] { vex.Message } }
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Approve and process a draft adjustment
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromQuery] string approvedBy = "System")
    {
        try
        {
            var result = await _service.ApproveAsync(id, approvedBy);
            return Ok(result);
        }
        catch (ValidationException vex)
        {
            return BadRequest(new
            {
                errors = new Dictionary<string, string[]>
                {
                    { vex.Field, new[] { vex.Message } }
                }
            });
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
    /// Cancel a draft adjustment
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            await _service.CancelAsync(id);
            return NoContent();
        }
        catch (ValidationException vex)
        {
            return BadRequest(new
            {
                errors = new Dictionary<string, string[]>
                {
                    { vex.Field, new[] { vex.Message } }
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete (disable) an adjustment
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (ValidationException vex)
        {
            return BadRequest(new
            {
                errors = new Dictionary<string, string[]>
                {
                    { vex.Field, new[] { vex.Message } }
                }
            });
        }
    }

    /// <summary>
    /// Validate stock for a potential adjustment
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateStock([FromBody] CreateStockAdjustmentDto dto)
    {
        var (isValid, errorMessage) = await _service.ValidateStockAsync(dto);
        return Ok(new { isValid, errorMessage });
    }
}
