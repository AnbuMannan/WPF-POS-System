using POS.Domain.Entities;
using POS.Domain.Enums;

namespace POS.Application.Interfaces.Repositories;

public interface IPurchaseOrderRepository
{
    Task<List<PurchaseOrder>> GetAllAsync(bool includeInactive = false);
    Task<PurchaseOrder?> GetByIdAsync(Guid id, bool includeItems = true);
    Task<List<PurchaseOrder>> GetPendingOrdersBySuppliersAsync(Guid supplierId);
    Task<List<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status);
    Task AddAsync(PurchaseOrder purchaseOrder);
    Task UpdateAsync(PurchaseOrder purchaseOrder);
    Task DisableAsync(Guid id);
    Task<bool> CheckReferenceNoExistsAsync(string referenceNo, Guid? excludeId);
}
