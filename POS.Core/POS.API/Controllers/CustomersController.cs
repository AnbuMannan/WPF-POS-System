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
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("all")]
    public async Task<IActionResult> GetAllAsync()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CustomerDto customer)
    {
        try
        {
            await _service.AddAsync(customer);
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
    public async Task<IActionResult> Update(CustomerDto customer)
    {
        try
        {
            await _service.UpdateAsync(customer);
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
    public async Task<IActionResult> Disable(string id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }

    [HttpGet("exists/phone")]
    public async Task<IActionResult> CheckPhoneExists(string phone, string? excludeId)
    {
        bool exists = await _service.CheckPhoneExistsAsync(phone, excludeId);
        return Ok(exists);
    }
}
