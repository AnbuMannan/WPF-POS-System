using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface ISupplierTransactionRepository
{
    Task<IEnumerable<SupplierTransaction>> GetAllAsync();
    Task<SupplierTransaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<SupplierTransaction>> GetBySupplierAsync(Guid supplierId);
    Task<SupplierTransaction?> GetLatestBySupplierAsync(Guid supplierId);
    Task<decimal> GetSupplierBalanceAsync(Guid supplierId);
    Task<SupplierTransaction> CreateAsync(SupplierTransaction transaction);
    Task<SupplierTransaction> CreateWithBalanceAsync(SupplierTransaction transaction);
    
    /// <summary>
    /// Gets transaction summary for all suppliers (for balance display)
    /// </summary>
    Task<IEnumerable<(Guid SupplierId, decimal TotalPurchases, decimal TotalReturns, decimal TotalPayments, decimal CurrentBalance, DateTime? LastTransactionDate)>> GetAllSupplierBalancesAsync();
    
    /// <summary>
    /// Gets transactions for a supplier within a date range
    /// </summary>
    Task<IEnumerable<SupplierTransaction>> GetBySupplierAndDateRangeAsync(Guid supplierId, DateTime fromDate, DateTime toDate);
    
    /// <summary>
    /// Gets the balance for a supplier as of a specific date (sum of all transactions before the date)
    /// </summary>
    Task<decimal> GetBalanceAsOfDateAsync(Guid supplierId, DateTime asOfDate);
}
