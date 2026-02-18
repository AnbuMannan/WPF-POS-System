using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

/// <summary>
/// Repository for stock-related queries
/// </summary>
public class StockRepository : IStockRepository
{
    private readonly PosDbContext _context;

    public StockRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetProductStockAsync(long productId)
    {
        // Get stock from Batches (the primary source of stock info)
        var batchStock = await _context.Batches
            .Where(b => b.ProductId == productId && b.IsActive)
            .SumAsync(b => (decimal?)b.CurrentQuantity) ?? 0;

        return batchStock;
    }

    public async Task<StockSummary?> GetStockSummaryAsync(long productId)
    {
        return await _context.StockSummaries.FirstOrDefaultAsync(ss => ss.ProductId == productId);
    }

    public async Task<IEnumerable<(long ProductId, decimal Stock)>> GetAllProductStocksAsync()
    {
        // Get stock grouped by product from Batches
        var stocks = await _context.Batches
            .Where(b => b.IsActive)
            .GroupBy(b => b.ProductId)
            .Select(g => new { ProductId = g.Key, Stock = g.Sum(b => b.CurrentQuantity) })
            .ToListAsync();

        return stocks.Select(s => (s.ProductId, s.Stock));
    }

    public async Task<IEnumerable<(long ProductId, decimal Stock, decimal ReorderLevel)>> GetLowStockProductsAsync()
    {
        // Products with stock below 10 (default reorder level)
        var stocks = await _context.Batches
            .Where(b => b.IsActive)
            .GroupBy(b => b.ProductId)
            .Select(g => new { ProductId = g.Key, Stock = g.Sum(b => b.CurrentQuantity) })
            .Where(s => s.Stock < 10)
            .ToListAsync();

        return stocks.Select(s => (s.ProductId, s.Stock, 10m)); // Default reorder level of 10
    }
}
