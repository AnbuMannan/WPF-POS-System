using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(long id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpGet("barcode/{code}")]
    public async Task<IActionResult> GetByBarcode(string code)
        => Ok(await _service.GetByBarcodeAsync(code));

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
        => Ok(await _service.SearchAsync(q));

    [HttpPost]
    public async Task<IActionResult> Create(ProductDto product)
    {
        try
        {
            await _service.AddAsync(product);
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
    public async Task<IActionResult> Update(ProductDto product)
    {
        try
        {
            await _service.UpdateAsync(product);
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
    public async Task<IActionResult> Disable(long id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllAsync(bool showInactive)
    {
        var products = await _service.GetAllAsync(showInactive);
        return Ok(products);
    }

    [HttpGet("exists/sku")]
    public async Task<IActionResult> CheckSku(string sku, long? excludeId)
    {
        bool exists = await _service.SKUExistsAsync(sku, excludeId);
        return Ok(exists);
    }

    [HttpGet("exists/barcode")]
    public async Task<IActionResult> CheckBarcode(string barcode, long? excludeId)
    {
        bool exists = await _service.BarcodeExistsAsync(barcode, excludeId);
        return Ok(exists);
    }

}
