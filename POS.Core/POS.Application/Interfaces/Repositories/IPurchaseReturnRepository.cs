using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IPurchaseReturnRepository
{
    Task<List<PurchaseReturn>> GetAllAsync(bool includeInactive = false);
    Task<PurchaseReturn?> GetByIdAsync(Guid id, bool includeItems = true);
    Task<List<PurchaseReturn>> GetBySupplierAsync(Guid supplierId);
    Task<List<PurchaseReturn>> GetByPurchaseEntryIdAsync(Guid purchaseEntryId);
    Task<List<PurchaseReturn>> GetUnprocessedAsync();
    Task AddAsync(PurchaseReturn purchaseReturn);
    Task UpdateAsync(PurchaseReturn purchaseReturn);
    Task DisableAsync(Guid id);
    Task<bool> CheckReturnNoExistsAsync(string returnNo, Guid? excludeId);
    
    /// <summary>
    /// Process purchase return with full inventory update (atomic transaction)
    /// Reduces stock and creates ledger entries
    /// </summary>
    Task ProcessReturnWithInventoryUpdateAsync(Guid purchaseReturnId);
}
