using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/purchase-returns")]
public class PurchaseReturnsController : ControllerBase
{
    private readonly IPurchaseReturnService _service;

    public PurchaseReturnsController(IPurchaseReturnService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var returnEntry = await _service.GetByIdAsync(id);
        if (returnEntry == null)
            return NotFound();
        return Ok(returnEntry);
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplier(Guid supplierId)
        => Ok(await _service.GetBySupplierAsync(supplierId));

    [HttpGet("purchase-entry/{purchaseEntryId}")]
    public async Task<IActionResult> GetByPurchaseEntry(Guid purchaseEntryId)
        => Ok(await _service.GetByPurchaseEntryIdAsync(purchaseEntryId));

    [HttpGet("unprocessed")]
    public async Task<IActionResult> GetUnprocessed()
        => Ok(await _service.GetUnprocessedAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseReturnDto dto)
    {
        try
        {
            int storeCode = Request.Headers.TryGetValue("X-Store-Code", out var scVal) && int.TryParse(scVal, out int parsedSc) ? parsedSc : 1;
            var created = await _service.CreateAsync(dto, storeCode);
            return Ok(created);
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreatePurchaseReturnDto dto)
    {
        try
        {
            int storeCode = Request.Headers.TryGetValue("X-Store-Code", out var scVal) && int.TryParse(scVal, out int parsedSc) ? parsedSc : 1;
            var updated = await _service.UpdateAsync(id, dto, storeCode);
            return Ok(updated);
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
    /// CRITICAL ENDPOINT: Process purchase return and update inventory
    /// Reduces stock from batches and creates ledger entries
    /// This uses transactions internally to ensure atomicity
    /// </summary>
    [HttpPost("{id}/process")]
    public async Task<IActionResult> ProcessReturn(Guid id)
    {
        try
        {
            int storeCode = Request.Headers.TryGetValue("X-Store-Code", out var scVal) && int.TryParse(scVal, out int parsedSc) ? parsedSc : 1;
            var processed = await _service.ProcessReturnAsync(id, storeCode);
            return Ok(processed);
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
        try
        {
            await _service.DisableAsync(id);
            return Ok(new { message = "Purchase return disabled successfully" });
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

    [HttpGet("check-return-no/{returnNo}")]
    public async Task<IActionResult> CheckReturnNoExists(string returnNo, [FromQuery] Guid? excludeId = null)
    {
        var exists = await _service.CheckReturnNoExistsAsync(returnNo, excludeId);
        return Ok(new { exists });
    }
}
