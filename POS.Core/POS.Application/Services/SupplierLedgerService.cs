using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.Application.Services;

public class SupplierLedgerService : ISupplierLedgerService
{
    private readonly ISupplierTransactionRepository _transactionRepo;
    private readonly ISupplierRepository _supplierRepo;

    public SupplierLedgerService(
        ISupplierTransactionRepository transactionRepo,
        ISupplierRepository supplierRepo)
    {
        _transactionRepo = transactionRepo;
        _supplierRepo = supplierRepo;
    }

    public async Task<SupplierLedgerReportDto> GetLedgerReportAsync(Guid supplierId, DateTime fromDate, DateTime toDate)
    {
        // Get supplier details
        var supplier = await _supplierRepo.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new InvalidOperationException($"Supplier with ID '{supplierId}' not found.");

        // Get opening balance (balance before fromDate)
        var openingBalance = await _transactionRepo.GetBalanceAsOfDateAsync(supplierId, fromDate);

        // Get transactions within the date range
        var transactions = await _transactionRepo.GetBySupplierAndDateRangeAsync(supplierId, fromDate, toDate);

        // Build ledger entries with running balance
        var entries = new List<SupplierLedgerEntryDto>();
        decimal runningBalance = openingBalance;

        foreach (var txn in transactions)
        {
            // Calculate running balance: Credit increases, Debit decreases
            runningBalance = runningBalance + txn.CreditAmount - txn.DebitAmount;

            entries.Add(new SupplierLedgerEntryDto
            {
                Id = txn.Id,
                Date = txn.TransactionDate,
                Description = txn.Description ?? GetDefaultDescription(txn.TransactionType),
                TransactionType = txn.TransactionType,
                ReferenceNo = txn.ReferenceNo,
                DebitAmount = txn.DebitAmount,
                CreditAmount = txn.CreditAmount,
                RunningBalance = runningBalance
            });
        }

        // Calculate totals
        decimal totalDebit = entries.Sum(e => e.DebitAmount);
        decimal totalCredit = entries.Sum(e => e.CreditAmount);
        decimal closingBalance = openingBalance + totalCredit - totalDebit;

        return new SupplierLedgerReportDto
        {
            SupplierId = supplier.Id,
            SupplierName = supplier.Name,
            SupplierCode = supplier.Code,
            ContactPerson = supplier.ContactPerson,
            Mobile = supplier.Mobile,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            Entries = entries
        };
    }

    public async Task<List<SupplierLedgerEntryDto>> GetLedgerEntriesAsync(Guid supplierId, DateTime fromDate, DateTime toDate)
    {
        // Get opening balance (balance before fromDate)
        var openingBalance = await _transactionRepo.GetBalanceAsOfDateAsync(supplierId, fromDate);

        // Get transactions within the date range
        var transactions = await _transactionRepo.GetBySupplierAndDateRangeAsync(supplierId, fromDate, toDate);

        // Build ledger entries with running balance
        var entries = new List<SupplierLedgerEntryDto>();
        decimal runningBalance = openingBalance;

        foreach (var txn in transactions)
        {
            runningBalance = runningBalance + txn.CreditAmount - txn.DebitAmount;

            entries.Add(new SupplierLedgerEntryDto
            {
                Id = txn.Id,
                Date = txn.TransactionDate,
                Description = txn.Description ?? GetDefaultDescription(txn.TransactionType),
                TransactionType = txn.TransactionType,
                ReferenceNo = txn.ReferenceNo,
                DebitAmount = txn.DebitAmount,
                CreditAmount = txn.CreditAmount,
                RunningBalance = runningBalance
            });
        }

        return entries;
    }

    public async Task<decimal> GetBalanceAsOfDateAsync(Guid supplierId, DateTime asOfDate)
    {
        return await _transactionRepo.GetBalanceAsOfDateAsync(supplierId, asOfDate);
    }

    private static string GetDefaultDescription(string transactionType)
    {
        return transactionType switch
        {
            "Purchase" => "Purchase Entry",
            "PurchaseReturn" => "Purchase Return",
            "Payment" => "Supplier Payment",
            _ => transactionType
        };
    }
}
