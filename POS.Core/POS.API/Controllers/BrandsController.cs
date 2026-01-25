using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

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
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(Brand brand)
    {
        await _service.AddAsync(brand);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(Brand brand)
    {
        await _service.UpdateAsync(brand);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }
}
