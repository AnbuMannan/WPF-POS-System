using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/purchase-entries")]
public class PurchaseEntriesController : ControllerBase
{
    private readonly IPurchaseEntryService _service;

    public PurchaseEntriesController(IPurchaseEntryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var entry = await _service.GetByIdAsync(id);
        if (entry == null)
            return NotFound();
        return Ok(entry);
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplier(Guid supplierId)
        => Ok(await _service.GetBySupplierAsync(supplierId));

    [HttpGet("unprocessed")]
    public async Task<IActionResult> GetUnprocessed()
        => Ok(await _service.GetUnprocessedAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseEntryDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
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
    public async Task<IActionResult> Update(Guid id, CreatePurchaseEntryDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
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
    /// CRITICAL ENDPOINT: Process purchase entry and update inventory
    /// This uses transactions internally to ensure atomicity
    /// </summary>
    [HttpPost("{id}/process")]
    public async Task<IActionResult> ProcessEntry(Guid id, [FromQuery] bool updateProductPrices = true)
    {
        try
        {
            var processed = await _service.ProcessEntryAsync(id, updateProductPrices);
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
            var result = await _service.DisableAsync(id);
            if (!result)
                return NotFound();
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

    [HttpGet("exists/invoice")]
    public async Task<IActionResult> CheckInvoiceNoExists(string invoiceNo, Guid? excludeId)
    {
        bool exists = await _service.CheckInvoiceNoExistsAsync(invoiceNo, excludeId);
        return Ok(exists);
    }
}
