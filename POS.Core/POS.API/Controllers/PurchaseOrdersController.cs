using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Domain.Enums;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrdersController(IPurchaseOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var po = await _service.GetByIdAsync(id);
        if (po == null)
            return NotFound();
        return Ok(po);
    }

    [HttpGet("supplier/{supplierId}/pending")]
    public async Task<IActionResult> GetPendingBySupplier(Guid supplierId)
        => Ok(await _service.GetPendingOrdersBySupplierAsync(supplierId));

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(PurchaseOrderStatus status)
        => Ok(await _service.GetByStatusAsync(status));

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseOrderDto dto)
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
    public async Task<IActionResult> Update(Guid id, CreatePurchaseOrderDto dto)
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

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] PurchaseOrderStatus status)
    {
        var result = await _service.UpdateStatusAsync(id, status);
        if (!result)
            return NotFound();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }

    [HttpGet("exists/reference")]
    public async Task<IActionResult> CheckReferenceNoExists(string referenceNo, Guid? excludeId)
    {
        bool exists = await _service.CheckReferenceNoExistsAsync(referenceNo, excludeId);
        return Ok(exists);
    }
}
