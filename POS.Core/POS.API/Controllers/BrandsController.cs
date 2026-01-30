using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _service;

    public BrandsController(IBrandService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("all")]
    public async Task<IActionResult> GetAllAsync([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(BrandDto brand)
    {
        try
        {
            await _service.AddAsync(brand);
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
    public async Task<IActionResult> Update(BrandDto brand)
    {
        try
        {
            await _service.UpdateAsync(brand);
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
    public async Task<IActionResult> Disable(int id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }

    [HttpGet("exists/name")]
    public async Task<IActionResult> CheckNameExists(string name, int? excludeId)
    {
        bool exists = await _service.CheckNameExistsAsync(name, excludeId);
        return Ok(exists);
    }
}
