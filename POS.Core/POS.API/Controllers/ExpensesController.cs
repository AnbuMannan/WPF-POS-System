using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Infrastructure.Data;
using POS.Shared.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly PosDbContext _context;

    public ExpensesController(PosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetExpenses([FromQuery] DateTime? date)
    {
        var query = _context.Set<Expense>().AsQueryable();

        if (date.HasValue)
        {
            var startDate = date.Value.Date;
            var endDate = startDate.AddDays(1);
            // A1-Grade: Safely captures anything from 00:00:00 to 23:59:59
            query = query.Where(e => e.ExpenseDate >= startDate && e.ExpenseDate < endDate && e.IsActive == true);
        }

        var expenses = await query.ToListAsync();

        var dtos = expenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            StoreCode = e.StoreCode,
            ExpenseDate = e.ExpenseDate,
            Category = e.Category,
            Description = e.Description,
            Amount = e.Amount,
            CreatedBy = e.CreatedBy
        }).ToList();

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ExpenseDto dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Invalid payload" });
        if (string.IsNullOrWhiteSpace(dto.Category))
            return BadRequest(new { message = "Category is required" });
        if (dto.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero" });

        // Extract StoreCode from the JWT token (Fallback to 1 for development if missing)
        var storeCodeClaim = User.Claims.FirstOrDefault(c => c.Type == "StoreCode")?.Value;
        int storeCode = int.TryParse(storeCodeClaim, out int sc) ? sc : 1;

        var expense = new Expense
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            StoreCode = storeCode,
            ExpenseDate = dto.ExpenseDate == default ? DateTime.Now : dto.ExpenseDate,
            Category = dto.Category,
            Description = dto.Description ?? string.Empty,
            Amount = dto.Amount,
            CreatedBy = dto.CreatedBy ?? "System",
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.Set<Expense>().Add(expense);
        await _context.SaveChangesAsync();

        dto.Id = expense.Id;
        dto.StoreCode = expense.StoreCode;
        return CreatedAtAction(nameof(GetExpenses), new { id = expense.Id }, dto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var expense = await _context.Set<Expense>().FindAsync(id);
        if (expense == null) return NotFound();

        // Soft delete ensures audit history is preserved
        expense.IsActive = false;
        expense.UpdatedAt = DateTime.Now; // Keep this line as it's good practice for soft deletes
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int ParseStoreCodeFromHeader()
    {
        if (Request.Headers.TryGetValue("X-Store-Code", out var values))
        {
            if (int.TryParse(values.FirstOrDefault(), out int code))
                return code;
        }
        return 0;
    }
}
