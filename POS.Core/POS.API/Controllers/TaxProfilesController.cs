using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

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
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(TaxProfile taxProfile)
    {
        await _service.AddAsync(taxProfile);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(TaxProfile taxProfile)
    {
        await _service.UpdateAsync(taxProfile);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }
}
