using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomersController(ICustomerService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var dto = await _service.GetByIdAsync(id);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CustomerDto dto)
    {
        try
        {
            await _service.AddAsync(dto);
            return Ok();
        }
        catch (ValidationException vex)
        {
            return BadRequest(new { errors = new Dictionary<string, string[]> { { vex.Field, new[] { vex.Message } } } });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update(CustomerDto dto)
    {
        try
        {
            await _service.UpdateAsync(dto);
            return Ok();
        }
        catch (ValidationException vex)
        {
            return BadRequest(new { errors = new Dictionary<string, string[]> { { vex.Field, new[] { vex.Message } } } });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }

    [HttpGet("exists/phone")]
    public async Task<IActionResult> CheckPhoneExists([FromQuery] string? phone, [FromQuery] Guid? excludeId)
    {
        var exists = await _service.CheckPhoneExistsAsync(phone, excludeId);
        return Ok(exists);
    }
}
