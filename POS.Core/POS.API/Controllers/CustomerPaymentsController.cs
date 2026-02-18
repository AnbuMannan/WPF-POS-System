using Microsoft.AspNetCore.Mvc;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/customer-payments")]
public class CustomerPaymentsController : ControllerBase
{
    private readonly ICustomerPaymentService _service;

    public CustomerPaymentsController(ICustomerPaymentService service)
    {
        _service = service;
    }

    [HttpGet("outstanding")]
    public async Task<ActionResult<List<CustomerBalanceDto>>> GetOutstandingCustomers()
    {
        var result = await _service.GetCustomersWithOutstandingAsync();
        return Ok(result);
    }

    [HttpGet("{customerId}/balance")]
    public async Task<ActionResult<decimal>> GetBalance(Guid customerId)
    {
        var balance = await _service.GetOutstandingAsync(customerId);
        return Ok(new { balance });
    }

    [HttpGet("{customerId}/ledger")]
    public async Task<ActionResult<CustomerLedgerDto>> GetLedger(Guid customerId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var result = await _service.GetCustomerLedgerAsync(customerId, fromDate, toDate);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("pay")]
    public async Task<ActionResult<CustomerTransactionDto>> PayDue([FromBody] CustomerPaymentRequestDto dto)
    {
        try
        {
            var result = await _service.PayDueAsync(dto);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
