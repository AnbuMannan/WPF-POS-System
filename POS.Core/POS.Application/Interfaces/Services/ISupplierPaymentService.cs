using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ISupplierPaymentService
{
    Task<IEnumerable<SupplierPaymentDto>> GetAllAsync(bool includeInactive = false);
    Task<SupplierPaymentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<SupplierPaymentDto>> GetBySupplierAsync(Guid supplierId);
    Task<SupplierPaymentDto> CreateAsync(CreateSupplierPaymentDto dto, int storeCode);
    Task<SupplierPaymentDto> UpdateAsync(Guid id, CreateSupplierPaymentDto dto, int storeCode);
    Task<bool> DisableAsync(Guid id);
    Task<bool> PaymentNoExistsAsync(string paymentNo, Guid? excludeId = null);
}
