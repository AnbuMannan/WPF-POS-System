using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

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
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        await _service.AddAsync(category);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(Category category)
    {
        await _service.UpdateAsync(category);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }

    [HttpGet("exists")]
    public async Task<IActionResult> CheckNameExists(string name, Guid? parentCategoryId, Guid? excludeId)
    {
        var exists = await _service.CheckNameExistsAsync(name, parentCategoryId, excludeId);
        return Ok(exists);
    }


}
