using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IStockAdjustmentRepository
{
    Task<IEnumerable<StockAdjustment>> GetAllAsync(bool includeInactive = false);
    Task<StockAdjustment?> GetByIdAsync(Guid id, bool includeItems = false);
    Task<StockAdjustment?> GetByReferenceNoAsync(string referenceNo);
    Task<IEnumerable<StockAdjustment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<IEnumerable<StockAdjustment>> GetByReasonAsync(string reason);
    Task<IEnumerable<StockAdjustment>> GetByStatusAsync(string status);
    Task<string> GenerateReferenceNoAsync();
    Task<StockAdjustment> CreateAsync(StockAdjustment adjustment);
    Task<StockAdjustment> UpdateAsync(StockAdjustment adjustment);
    Task ApproveAsync(Guid id, string approvedBy);
    Task CancelAsync(Guid id);
    Task DisableAsync(Guid id);
    
    /// <summary>
    /// Process the adjustment with inventory updates in a transaction
    /// </summary>
    Task ProcessAdjustmentWithInventoryAsync(Guid id);
}
