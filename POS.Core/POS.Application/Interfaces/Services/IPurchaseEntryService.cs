using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IPurchaseEntryService
{
    Task<List<PurchaseEntryDto>> GetAllAsync(bool includeInactive = false);
    Task<PurchaseEntryDto?> GetByIdAsync(Guid id);
    Task<List<PurchaseEntryDto>> GetBySupplierAsync(Guid supplierId);
    Task<List<PurchaseEntryDto>> GetUnprocessedAsync();
    Task<PurchaseEntryDto> CreateAsync(CreatePurchaseEntryDto dto, int storeCode);
    Task<PurchaseEntryDto> UpdateAsync(Guid id, CreatePurchaseEntryDto dto, int storeCode);
    Task<PurchaseEntryDto> ProcessEntryAsync(Guid id, int storeCode, bool updateProductPrices = true);
    Task<bool> DisableAsync(Guid id);
    Task<bool> CheckInvoiceNoExistsAsync(string invoiceNo, Guid? excludeId);
}
