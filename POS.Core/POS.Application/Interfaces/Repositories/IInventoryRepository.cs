using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IInventoryRepository
{
    Task AddLedgerEntryAsync(StockLedgerEntry entry);
    Task<StockSummary> GetStockAsync(long productId);
    Task UpdateStockAsync(long productId, decimal delta);
}
