using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ISupplierTransactionService
{
    Task<IEnumerable<SupplierTransactionDto>> GetBySupplierAsync(Guid supplierId);
    Task<decimal> GetSupplierBalanceAsync(Guid supplierId);
    Task<IEnumerable<SupplierBalanceDto>> GetAllSupplierBalancesAsync();
    
    /// <summary>
    /// Records a purchase transaction (Credit to supplier - increases balance)
    /// </summary>
    Task RecordPurchaseAsync(Guid supplierId, Guid purchaseEntryId, string invoiceNo, decimal amount, int storeCode, string? description = null);
    
    /// <summary>
    /// Records a purchase return transaction (Debit to supplier - decreases balance)
    /// </summary>
    Task RecordPurchaseReturnAsync(Guid supplierId, Guid purchaseReturnId, string returnNo, decimal amount, int storeCode, string? description = null);
    
    /// <summary>
    /// Records a payment transaction (Debit to supplier - decreases balance)
    /// </summary>
    Task RecordPaymentAsync(Guid supplierId, Guid paymentId, string paymentNo, decimal amount, int storeCode, string? description = null);
}
