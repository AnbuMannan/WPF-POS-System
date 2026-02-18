using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/sale-returns")]
public class SaleReturnsController : ControllerBase
{
    private readonly ISaleReturnService _service;

    public SaleReturnsController(ISaleReturnService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<SaleReturnDto>>> GetAll()
    {
        var returns = await _service.GetAllAsync();
        return Ok(returns);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaleReturnDto>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("lookup-invoice/{billNumber}")]
    public async Task<ActionResult<SaleInvoiceForReturnDto>> LookupInvoice(string billNumber)
    {
        var result = await _service.LookupInvoiceAsync(billNumber);
        if (result == null) return NotFound(new { message = "Invoice not found." });
        return Ok(result);
    }

    [HttpGet("lookup-invoice-by-id/{saleId}")]
    public async Task<ActionResult<SaleInvoiceForReturnDto>> LookupInvoiceBySaleId(long saleId)
    {
        var result = await _service.LookupInvoiceBySaleIdAsync(saleId);
        if (result == null) return NotFound(new { message = "Invoice not found." });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SaleReturnDto>> Create([FromBody] CreateSaleReturnDto dto)
    {
        try
        {
            var result = await _service.CreateReturnAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.ReturnId }, result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/process")]
    public async Task<ActionResult<SaleReturnDto>> Process(int id)
    {
        try
        {
            var result = await _service.ProcessReturnAsync(id);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
