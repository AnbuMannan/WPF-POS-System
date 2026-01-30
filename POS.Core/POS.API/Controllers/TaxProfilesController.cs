using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/taxprofiles")]
public class TaxProfilesController : ControllerBase
{
    private readonly ITaxProfileService _service;

    public TaxProfilesController(ITaxProfileService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _service.GetAllAsync(includeInactive));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(TaxProfileDto dto)
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
    public async Task<IActionResult> Update(TaxProfileDto dto)
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Disable(int id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }
}
