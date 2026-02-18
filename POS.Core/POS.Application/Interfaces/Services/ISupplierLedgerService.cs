using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ISupplierLedgerService
{
    /// <summary>
    /// Get supplier ledger report with opening/closing balances for a date range
    /// </summary>
    /// <param name="supplierId">The supplier ID</param>
    /// <param name="fromDate">Start date (inclusive)</param>
    /// <param name="toDate">End date (inclusive)</param>
    /// <returns>Complete ledger report with running balances</returns>
    Task<SupplierLedgerReportDto> GetLedgerReportAsync(Guid supplierId, DateTime fromDate, DateTime toDate);
    
    /// <summary>
    /// Get supplier ledger entries for a date range (without opening/closing balance calculation)
    /// </summary>
    Task<List<SupplierLedgerEntryDto>> GetLedgerEntriesAsync(Guid supplierId, DateTime fromDate, DateTime toDate);
    
    /// <summary>
    /// Get the balance for a supplier as of a specific date
    /// </summary>
    Task<decimal> GetBalanceAsOfDateAsync(Guid supplierId, DateTime asOfDate);
}
