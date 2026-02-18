using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for StockLedgerEntry operations
/// </summary>
public class StockLedgerRepository : IStockLedgerRepository
{
    private readonly PosDbContext _context;

    public StockLedgerRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockLedgerEntry>> GetByProductIdAsync(long productId)
    {
        return await _context.Set<StockLedgerEntry>()
            .Where(e => e.ProductId == productId)
            .OrderBy(e => e.EntryDate)
            .ThenBy(e => e.StockEntryId)
            .ToListAsync();
    }

    public async Task<List<StockLedgerEntry>> GetByProductIdAndDateRangeAsync(long productId, DateTime fromDate, DateTime toDate)
    {
        var toDateEnd = toDate.Date.AddDays(1).AddTicks(-1);
        
        return await _context.Set<StockLedgerEntry>()
            .Where(e => e.ProductId == productId && 
                       e.EntryDate >= fromDate.Date && 
                       e.EntryDate <= toDateEnd)
            .OrderBy(e => e.EntryDate)
            .ThenBy(e => e.StockEntryId)
            .ToListAsync();
    }
}
