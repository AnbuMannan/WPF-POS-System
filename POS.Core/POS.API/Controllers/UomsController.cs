using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

namespace POS.API.Controllers;

[ApiController]
[Route("api/uoms")]
public class UomsController : ControllerBase
{
    private readonly IUomService _service;

    public UomsController(IUomService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(Uom uom)
    {
        await _service.AddAsync(uom);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(Uom uom)
    {
        await _service.UpdateAsync(uom);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }

    [HttpGet("exists/code")]
    public async Task<IActionResult> CheckCode(string code, Guid? excludeId)
    {
        var exists = await _service.CodeExistsAsync(code, excludeId);
        return Ok(exists);
    }
}
