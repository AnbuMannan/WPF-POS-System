using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IPurchaseEntryRepository
{
    Task<List<PurchaseEntry>> GetAllAsync(bool includeInactive = false);
    Task<PurchaseEntry?> GetByIdAsync(Guid id, bool includeItems = true);
    Task<List<PurchaseEntry>> GetBySuppliersAsync(Guid supplierId);
    Task<PurchaseEntry?> GetByPurchaseOrderIdAsync(Guid purchaseOrderId);
    Task<List<PurchaseEntry>> GetUnprocessedAsync();
    Task AddAsync(PurchaseEntry purchaseEntry);
    Task UpdateAsync(PurchaseEntry purchaseEntry);
    Task DisableAsync(Guid id);
    Task<bool> CheckInvoiceNoExistsAsync(string invoiceNo, Guid? excludeId);
    
    /// <summary>
    /// Process purchase entry with full inventory update (atomic transaction)
    /// </summary>
    Task ProcessEntryWithInventoryUpdateAsync(Guid purchaseEntryId, bool updateProductPrices);
}
