using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/cash-transactions")]
    public class CashTransactionsController : ControllerBase
    {
        private readonly ICashTransactionService _service;

        public CashTransactionsController(ICashTransactionService service)
        {
            _service = service;
        }

        // GET: api/cash-transactions
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var transactions = await _service.GetAllAsync(fromDate, toDate);
            return Ok(transactions);
        }

        // GET: api/cash-transactions/today
        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            var transactions = await _service.GetTodayAsync();
            return Ok(transactions);
        }

        // GET: api/cash-transactions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var transaction = await _service.GetByIdAsync(id);
            if (transaction == null)
                return NotFound(new { message = "Transaction not found" });
            return Ok(transaction);
        }

        // GET: api/cash-transactions/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var summary = await _service.GetSummaryAsync(fromDate, toDate);
            return Ok(summary);
        }

        // GET: api/cash-transactions/summary/today
        [HttpGet("summary/today")]
        public async Task<IActionResult> GetTodaySummary()
        {
            var summary = await _service.GetTodaySummaryAsync();
            return Ok(summary);
        }

        // POST: api/cash-transactions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCashTransactionDto dto, [FromQuery] int userId = 1, [FromQuery] string userName = "System")
        {
            try
            {
                var transaction = await _service.AddAsync(dto, userId, userName);
                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/cash-transactions/cash-in
        [HttpPost("cash-in")]
        public async Task<IActionResult> CashIn([FromBody] CreateCashTransactionDto dto, [FromQuery] int userId = 1, [FromQuery] string userName = "System")
        {
            try
            {
                dto.Type = "CashIn";
                var transaction = await _service.AddAsync(dto, userId, userName);
                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/cash-transactions/cash-out
        [HttpPost("cash-out")]
        public async Task<IActionResult> CashOut([FromBody] CreateCashTransactionDto dto, [FromQuery] int userId = 1, [FromQuery] string userName = "System")
        {
            try
            {
                dto.Type = "CashOut";
                var transaction = await _service.AddAsync(dto, userId, userName);
                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
