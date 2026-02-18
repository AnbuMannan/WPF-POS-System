using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IBatchRepository
{
    Task<List<Batch>> GetAllAsync(bool includeInactive = false);
    Task<Batch?> GetByIdAsync(Guid id);
    Task<List<Batch>> GetByProductIdAsync(long productId);
    Task<List<Batch>> GetByBatchNoAsync(string batchNo);
    Task<List<Batch>> GetAvailableBatchesAsync(long productId);
    Task<List<Batch>> GetExpiredBatchesAsync();
    Task<List<Batch>> GetExpiringBatchesAsync(int daysThreshold = 30);
    Task<Batch?> GetByPurchaseEntryItemAsync(Guid purchaseEntryItemId);
    Task AddAsync(Batch batch);
    Task UpdateAsync(Batch batch);
    Task DisableAsync(Guid id);
    Task<decimal> GetTotalStockForProductAsync(long productId);
    Task<decimal> GetAvailableStockForProductAsync(long productId);
}
