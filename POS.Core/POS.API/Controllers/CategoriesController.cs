using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
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
    public async Task<IActionResult> Create(CategoryDto category)
    {
        await _service.AddAsync(category);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(CategoryDto category)
    {
        await _service.UpdateAsync(category);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Disable(int id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }

    [HttpGet("exists")]
    public async Task<IActionResult> CheckNameExists(string name, int? parentCategoryId, int? excludeId)
    {
        var exists = await _service.CheckNameExistsAsync(name, parentCategoryId, excludeId);
        return Ok(exists);
    }


}
