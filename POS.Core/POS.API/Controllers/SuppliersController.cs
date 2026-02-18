using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _service;
    private readonly ISupplierLedgerService _ledgerService;

    public SuppliersController(ISupplierService service, ISupplierLedgerService ledgerService)
    {
        _service = service;
        _ledgerService = ledgerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("all")]
    public async Task<IActionResult> GetAllAsync([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(SupplierDto supplier)
    {
        try
        {
            await _service.AddAsync(supplier);
            return Ok();
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

    [HttpPut]
    public async Task<IActionResult> Update(SupplierDto supplier)
    {
        try
        {
            await _service.UpdateAsync(supplier);
            return Ok();
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }

    [HttpGet("exists/code")]
    public async Task<IActionResult> CheckCodeExists(string code, Guid? excludeId)
    {
        bool exists = await _service.CheckCodeExistsAsync(code, excludeId);
        return Ok(exists);
    }

    /// <summary>
    /// Get supplier ledger report with opening/closing balances
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <param name="fromDate">Start date (inclusive)</param>
    /// <param name="toDate">End date (inclusive)</param>
    [HttpGet("{id}/ledger")]
    public async Task<ActionResult<SupplierLedgerReportDto>> GetLedger(
        Guid id,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var report = await _ledgerService.GetLedgerReportAsync(id, fromDate, toDate);
            return Ok(report);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get supplier balance as of a specific date
    /// </summary>
    [HttpGet("{id}/balance")]
    public async Task<ActionResult<decimal>> GetBalanceAsOfDate(
        Guid id,
        [FromQuery] DateTime? asOfDate = null)
    {
        var date = asOfDate ?? DateTime.Now.AddDays(1); // Include today
        var balance = await _ledgerService.GetBalanceAsOfDateAsync(id, date);
        return Ok(balance);
    }
}
