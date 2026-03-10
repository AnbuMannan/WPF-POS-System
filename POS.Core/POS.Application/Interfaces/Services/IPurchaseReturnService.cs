using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IPurchaseReturnService
{
    Task<List<PurchaseReturnDto>> GetAllAsync(bool includeInactive = false);
    Task<PurchaseReturnDto?> GetByIdAsync(Guid id);
    Task<List<PurchaseReturnDto>> GetBySupplierAsync(Guid supplierId);
    Task<List<PurchaseReturnDto>> GetByPurchaseEntryIdAsync(Guid purchaseEntryId);
    Task<List<PurchaseReturnDto>> GetUnprocessedAsync();
    Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto dto, int storeCode);
    Task<PurchaseReturnDto> UpdateAsync(Guid id, CreatePurchaseReturnDto dto, int storeCode);
    Task<PurchaseReturnDto> ProcessReturnAsync(Guid id, int storeCode);
    Task<bool> DisableAsync(Guid id);
    Task<bool> CheckReturnNoExistsAsync(string returnNo, Guid? excludeId);
}
