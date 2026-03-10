using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupplierPaymentsController : ControllerBase
{
    private readonly ISupplierPaymentService _paymentService;
    private readonly ISupplierTransactionService _transactionService;

    public SupplierPaymentsController(
        ISupplierPaymentService paymentService,
        ISupplierTransactionService transactionService)
    {
        _paymentService = paymentService;
        _transactionService = transactionService;
    }

    /// <summary>
    /// Get all supplier payments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierPaymentDto>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var payments = await _paymentService.GetAllAsync(includeInactive);
        return Ok(payments);
    }

    /// <summary>
    /// Get supplier payment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierPaymentDto>> GetById(Guid id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
            return NotFound();
        return Ok(payment);
    }

    /// <summary>
    /// Get payments by supplier
    /// </summary>
    [HttpGet("supplier/{supplierId}")]
    public async Task<ActionResult<IEnumerable<SupplierPaymentDto>>> GetBySupplier(Guid supplierId)
    {
        var payments = await _paymentService.GetBySupplierAsync(supplierId);
        return Ok(payments);
    }

    /// <summary>
    /// Create a new supplier payment
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SupplierPaymentDto>> Create([FromBody] CreateSupplierPaymentDto dto)
    {
        try
        {
            int storeCode = Request.Headers.TryGetValue("X-Store-Code", out var scVal) && int.TryParse(scVal, out int parsedSc) ? parsedSc : 1;
            var payment = await _paymentService.CreateAsync(dto, storeCode);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update supplier payment (non-financial fields only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierPaymentDto>> Update(Guid id, [FromBody] CreateSupplierPaymentDto dto)
    {
        try
        {
            int storeCode = Request.Headers.TryGetValue("X-Store-Code", out var scVal) && int.TryParse(scVal, out int parsedSc) ? parsedSc : 1;
            var payment = await _paymentService.UpdateAsync(id, dto, storeCode);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Disable/soft delete a payment
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Disable(Guid id)
    {
        var result = await _paymentService.DisableAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    // ===== Supplier Transaction/Ledger Endpoints =====

    /// <summary>
    /// Get supplier ledger/transactions
    /// </summary>
    [HttpGet("ledger/{supplierId}")]
    public async Task<ActionResult<IEnumerable<SupplierTransactionDto>>> GetLedger(Guid supplierId)
    {
        var transactions = await _transactionService.GetBySupplierAsync(supplierId);
        return Ok(transactions);
    }

    /// <summary>
    /// Get supplier balance
    /// </summary>
    [HttpGet("balance/{supplierId}")]
    public async Task<ActionResult<decimal>> GetBalance(Guid supplierId)
    {
        var balance = await _transactionService.GetSupplierBalanceAsync(supplierId);
        return Ok(balance);
    }

    /// <summary>
    /// Get all supplier balances (for dashboard/list view)
    /// </summary>
    [HttpGet("balances")]
    public async Task<ActionResult<IEnumerable<SupplierBalanceDto>>> GetAllBalances()
    {
        var balances = await _transactionService.GetAllSupplierBalancesAsync();
        return Ok(balances);
    }
}
