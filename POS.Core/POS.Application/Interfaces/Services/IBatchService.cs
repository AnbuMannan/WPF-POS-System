using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IBatchService
{
    Task<List<BatchDto>> GetAllAsync(bool includeInactive = false);
    Task<BatchDto?> GetByIdAsync(Guid id);
    Task<List<BatchDto>> GetByProductIdAsync(long productId);
    Task<List<BatchDto>> GetAvailableBatchesAsync(long productId);
    Task<List<BatchDto>> GetExpiredBatchesAsync();
    Task<List<BatchDto>> GetExpiringBatchesAsync(int daysThreshold = 30);
    Task<decimal> GetTotalStockForProductAsync(long productId);
    Task<decimal> GetAvailableStockForProductAsync(long productId);
}
