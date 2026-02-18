using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IStockAdjustmentService
{
    Task<List<StockAdjustmentDto>> GetAllAsync(bool includeInactive = false);
    Task<StockAdjustmentDto?> GetByIdAsync(Guid id);
    Task<StockAdjustmentDto?> GetByReferenceNoAsync(string referenceNo);
    Task<List<StockAdjustmentDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<List<StockAdjustmentDto>> GetByReasonAsync(string reason);
    Task<List<StockAdjustmentDto>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Create and immediately approve/process a stock adjustment
    /// </summary>
    Task<StockAdjustmentDto> CreateAndApproveAsync(CreateStockAdjustmentDto dto, string approvedBy);
    
    /// <summary>
    /// Create a draft adjustment (not yet processed)
    /// </summary>
    Task<StockAdjustmentDto> CreateDraftAsync(CreateStockAdjustmentDto dto);
    
    /// <summary>
    /// Approve and process a draft adjustment
    /// </summary>
    Task<StockAdjustmentDto> ApproveAsync(Guid id, string approvedBy);
    
    /// <summary>
    /// Cancel a draft adjustment
    /// </summary>
    Task CancelAsync(Guid id);
    
    /// <summary>
    /// Delete (disable) an adjustment
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Validate stock availability for adjustment
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateStockAsync(CreateStockAdjustmentDto dto);
}
