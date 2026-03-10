using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class SupplierTransactionService : ISupplierTransactionService
{
    private readonly ISupplierTransactionRepository _transactionRepository;
    private readonly ISupplierRepository _supplierRepository;

    public SupplierTransactionService(
        ISupplierTransactionRepository transactionRepository,
        ISupplierRepository supplierRepository)
    {
        _transactionRepository = transactionRepository;
        _supplierRepository = supplierRepository;
    }

    public async Task<IEnumerable<SupplierTransactionDto>> GetBySupplierAsync(Guid supplierId)
    {
        var transactions = await _transactionRepository.GetBySupplierAsync(supplierId);
        return transactions.Select(MapToDto);
    }

    public async Task<decimal> GetSupplierBalanceAsync(Guid supplierId)
    {
        return await _transactionRepository.GetSupplierBalanceAsync(supplierId);
    }

    public async Task<IEnumerable<SupplierBalanceDto>> GetAllSupplierBalancesAsync()
    {
        var suppliers = await _supplierRepository.GetAllAsync(includeInactive: false);
        var balances = await _transactionRepository.GetAllSupplierBalancesAsync();
        var balanceDict = balances.ToDictionary(b => b.SupplierId);

        var result = new List<SupplierBalanceDto>();
        foreach (var supplier in suppliers)
        {
            var dto = new SupplierBalanceDto
            {
                SupplierId = supplier.Id,
                SupplierName = supplier.Name,
                SupplierCode = supplier.Code,
                Mobile = supplier.Mobile
            };

            if (balanceDict.TryGetValue(supplier.Id, out var balance))
            {
                dto.TotalPurchases = balance.TotalPurchases;
                dto.TotalReturns = balance.TotalReturns;
                dto.TotalPayments = balance.TotalPayments;
                dto.CurrentBalance = balance.CurrentBalance;
                dto.LastTransactionDate = balance.LastTransactionDate;
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task RecordPurchaseAsync(Guid supplierId, Guid purchaseEntryId, string invoiceNo, decimal amount, int storeCode, string? description = null)
    {
        var transaction = new SupplierTransaction
        {
            Id = Guid.NewGuid(),
            StoreCode = storeCode,
            SupplierId = supplierId,
            TransactionDate = DateTime.Now,
            TransactionType = "Purchase",
            ReferenceId = purchaseEntryId,
            ReferenceNo = invoiceNo,
            DebitAmount = 0,
            CreditAmount = amount,
            Description = description ?? $"Purchase Entry: {invoiceNo}",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsActive = true
        };

        await _transactionRepository.CreateWithBalanceAsync(transaction);
    }

    public async Task RecordPurchaseReturnAsync(Guid supplierId, Guid purchaseReturnId, string returnNo, decimal amount, int storeCode, string? description = null)
    {
        var transaction = new SupplierTransaction
        {
            Id = Guid.NewGuid(),
            StoreCode = storeCode,
            SupplierId = supplierId,
            TransactionDate = DateTime.Now,
            TransactionType = "PurchaseReturn",
            ReferenceId = purchaseReturnId,
            ReferenceNo = returnNo,
            DebitAmount = amount,
            CreditAmount = 0,
            Description = description ?? $"Purchase Return: {returnNo}",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsActive = true
        };

        await _transactionRepository.CreateWithBalanceAsync(transaction);
    }

    public async Task RecordPaymentAsync(Guid supplierId, Guid paymentId, string paymentNo, decimal amount, int storeCode, string? description = null)
    {
        var transaction = new SupplierTransaction
        {
            Id = Guid.NewGuid(),
            StoreCode = storeCode,
            SupplierId = supplierId,
            TransactionDate = DateTime.Now,
            TransactionType = "Payment",
            ReferenceId = paymentId,
            ReferenceNo = paymentNo,
            DebitAmount = amount,
            CreditAmount = 0,
            Description = description ?? $"Payment: {paymentNo}",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsActive = true
        };

        await _transactionRepository.CreateWithBalanceAsync(transaction);
    }

    private static SupplierTransactionDto MapToDto(SupplierTransaction transaction)
    {
        return new SupplierTransactionDto
        {
            Id = transaction.Id,
            SupplierId = transaction.SupplierId,
            SupplierName = transaction.Supplier?.Name,
            SupplierCode = transaction.Supplier?.Code,
            TransactionDate = transaction.TransactionDate,
            TransactionType = transaction.TransactionType,
            ReferenceId = transaction.ReferenceId,
            ReferenceNo = transaction.ReferenceNo,
            DebitAmount = transaction.DebitAmount,
            CreditAmount = transaction.CreditAmount,
            Balance = transaction.Balance,
            Description = transaction.Description,
            CreatedAt = transaction.CreatedAt
        };
    }
}
