using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface ISupplierPaymentRepository
{
    Task<IEnumerable<SupplierPayment>> GetAllAsync(bool includeInactive = false);
    Task<SupplierPayment?> GetByIdAsync(Guid id);
    Task<IEnumerable<SupplierPayment>> GetBySupplierAsync(Guid supplierId);
    Task<SupplierPayment> CreateAsync(SupplierPayment payment);
    Task<SupplierPayment> UpdateAsync(SupplierPayment payment);
    Task<bool> DisableAsync(Guid id);
    Task<string> GeneratePaymentNoAsync();
    Task<bool> PaymentNoExistsAsync(string paymentNo, Guid? excludeId = null);
}
