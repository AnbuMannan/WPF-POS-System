using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

namespace POS.API.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Invoice invoice)
    {
        if (invoice == null || invoice.Items == null || !invoice.Items.Any())
            return BadRequest("Invoice must have at least one item");

        var invoiceId = await _invoiceService.CreateInvoiceAsync(invoice);
        return Ok(new { InvoiceId = invoiceId });
    }
}
