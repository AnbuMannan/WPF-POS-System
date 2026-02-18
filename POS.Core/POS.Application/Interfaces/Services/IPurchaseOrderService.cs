using POS.Domain.Enums;
using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IPurchaseOrderService
{
    Task<List<PurchaseOrderDto>> GetAllAsync(bool includeInactive = false);
    Task<PurchaseOrderDto?> GetByIdAsync(Guid id);
    Task<List<PurchaseOrderDto>> GetPendingOrdersBySupplierAsync(Guid supplierId);
    Task<List<PurchaseOrderDto>> GetByStatusAsync(PurchaseOrderStatus status);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto);
    Task<PurchaseOrderDto> UpdateAsync(Guid id, CreatePurchaseOrderDto dto);
    Task<bool> UpdateStatusAsync(Guid id, PurchaseOrderStatus status);
    Task<bool> DisableAsync(Guid id);
    Task<bool> CheckReferenceNoExistsAsync(string referenceNo, Guid? excludeId);
}
