using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for stock-related queries
/// </summary>
public interface IStockRepository
{
    /// <summary>
    /// Get current stock quantity for a product
    /// </summary>
    Task<decimal> GetProductStockAsync(long productId);

    /// <summary>
    /// Get stock summary for a product
    /// </summary>
    Task<StockSummary?> GetStockSummaryAsync(long productId);

    /// <summary>
    /// Get all products with their stock levels
    /// </summary>
    Task<IEnumerable<(long ProductId, decimal Stock)>> GetAllProductStocksAsync();

    /// <summary>
    /// Get products with stock below reorder level
    /// </summary>
    Task<IEnumerable<(long ProductId, decimal Stock, decimal ReorderLevel)>> GetLowStockProductsAsync();
}
