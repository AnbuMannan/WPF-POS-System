using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

/// <summary>
/// Service interface for item ledger operations
/// </summary>
public interface IItemLedgerService
{
    /// <summary>
    /// Get item ledger for a specific product
    /// </summary>
    Task<ItemLedgerResponseDto> GetLedgerAsync(long productId, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Get ledger entries only (without summary)
    /// </summary>
    Task<List<ItemLedgerDto>> GetEntriesAsync(long productId, DateTime? fromDate = null, DateTime? toDate = null);
}
