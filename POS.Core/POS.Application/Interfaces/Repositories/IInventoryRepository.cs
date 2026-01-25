using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IInventoryRepository
{
    Task AddLedgerEntryAsync(StockLedgerEntry entry);
    Task<StockSummary> GetStockAsync(Guid productId);
    Task UpdateStockAsync(Guid productId, decimal delta);
}
