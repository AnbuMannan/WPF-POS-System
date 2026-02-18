using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for StockLedgerEntry operations
/// </summary>
public interface IStockLedgerRepository
{
    /// <summary>
    /// Get all ledger entries for a specific product
    /// </summary>
    Task<List<StockLedgerEntry>> GetByProductIdAsync(long productId);
    
    /// <summary>
    /// Get ledger entries for a product within a date range
    /// </summary>
    Task<List<StockLedgerEntry>> GetByProductIdAndDateRangeAsync(long productId, DateTime fromDate, DateTime toDate);
}
