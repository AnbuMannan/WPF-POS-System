using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/billing")]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        [HttpPost("sales")]
        public async Task<ActionResult<ReceiptDto>> CreateSale([FromBody] CreateSaleDto dto)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User?.Identity?.Name
                ?? "System";
            var receipt = await _billingService.CreateSaleAsync(dto, userId);
            return Ok(receipt);
        }

        [HttpGet("sales/{saleId}/receipt")]
        public async Task<ActionResult<ReceiptDto>> GetReceiptBySaleId(int saleId)
        {
            // Placeholder
            return Ok(new ReceiptDto { BillNumber = $"INV{saleId}" });
        }

        [HttpGet("sales/generate-bill-number")]
        public async Task<ActionResult> GenerateBillNumber()
        {
            var billNumber = $"INV{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
            return Ok(new { BillNumber = billNumber });
        }

        [HttpPost("held-bills")]
        public async Task<ActionResult<HeldBillDto>> HoldBill([FromBody] object dto)
        {
            // Placeholder
            return Ok(new HeldBillDto { Id = 1 });
        }

        [HttpGet("held-bills")]
        public async Task<ActionResult<List<HeldBillDto>>> GetHeldBills()
        {
            return Ok(new List<HeldBillDto>());
        }

        [HttpDelete("held-bills/{id}")]
        public async Task<IActionResult> DeleteHeldBill(int id)
        {
            return Ok();
        }

        [HttpPost("draft-bills")]
        public async Task<ActionResult<DraftBillDto>> SaveDraft([FromBody] object dto)
        {
            // Placeholder
            return Ok(new DraftBillDto { Id = 1 });
        }

        [HttpGet("draft-bills")]
        public async Task<ActionResult<List<DraftBillDto>>> GetDraftBills()
        {
            return Ok(new List<DraftBillDto>());
        }

        [HttpDelete("draft-bills/{id}")]
        public async Task<IActionResult> DeleteDraftBill(int id)
        {
            return Ok();
        }
    }
}
