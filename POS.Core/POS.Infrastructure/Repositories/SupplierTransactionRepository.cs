using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class SupplierTransactionRepository : ISupplierTransactionRepository
{
    private readonly PosDbContext _context;

    public SupplierTransactionRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SupplierTransaction>> GetAllAsync()
    {
        return await _context.SupplierTransactions
            .Include(t => t.Supplier)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupplierTransaction?> GetByIdAsync(Guid id)
    {
        return await _context.SupplierTransactions
            .Include(t => t.Supplier)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<SupplierTransaction>> GetBySupplierAsync(Guid supplierId)
    {
        return await _context.SupplierTransactions
            .Include(t => t.Supplier)
            .Where(t => t.SupplierId == supplierId)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupplierTransaction?> GetLatestBySupplierAsync(Guid supplierId)
    {
        return await _context.SupplierTransactions
            .Where(t => t.SupplierId == supplierId)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<decimal> GetSupplierBalanceAsync(Guid supplierId)
    {
        var latest = await GetLatestBySupplierAsync(supplierId);
        return latest?.Balance ?? 0;
    }

    public async Task<SupplierTransaction> CreateAsync(SupplierTransaction transaction)
    {
        _context.SupplierTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<SupplierTransaction> CreateWithBalanceAsync(SupplierTransaction transaction)
    {
        // Get current balance
        var currentBalance = await GetSupplierBalanceAsync(transaction.SupplierId);
        
        // Calculate new balance
        // Credit increases balance (money owed to supplier)
        // Debit decreases balance (money paid to supplier)
        transaction.Balance = currentBalance + transaction.CreditAmount - transaction.DebitAmount;
        
        _context.SupplierTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<IEnumerable<(Guid SupplierId, decimal TotalPurchases, decimal TotalReturns, decimal TotalPayments, decimal CurrentBalance, DateTime? LastTransactionDate)>> GetAllSupplierBalancesAsync()
    {
        var result = await _context.SupplierTransactions
            .GroupBy(t => t.SupplierId)
            .Select(g => new
            {
                SupplierId = g.Key,
                TotalPurchases = g.Where(t => t.TransactionType == "Purchase").Sum(t => t.CreditAmount),
                TotalReturns = g.Where(t => t.TransactionType == "PurchaseReturn").Sum(t => t.DebitAmount),
                TotalPayments = g.Where(t => t.TransactionType == "Payment").Sum(t => t.DebitAmount),
                LastTransactionDate = g.Max(t => (DateTime?)t.TransactionDate)
            })
            .ToListAsync();

        // Calculate current balance for each supplier
        var balanceList = new List<(Guid, decimal, decimal, decimal, decimal, DateTime?)>();
        foreach (var item in result)
        {
            var currentBalance = item.TotalPurchases - item.TotalReturns - item.TotalPayments;
            balanceList.Add((item.SupplierId, item.TotalPurchases, item.TotalReturns, item.TotalPayments, currentBalance, item.LastTransactionDate));
        }

        return balanceList;
    }

    public async Task<IEnumerable<SupplierTransaction>> GetBySupplierAndDateRangeAsync(Guid supplierId, DateTime fromDate, DateTime toDate)
    {
        // Include the entire day for toDate
        var toDateEnd = toDate.Date.AddDays(1).AddTicks(-1);
        
        return await _context.SupplierTransactions
            .Include(t => t.Supplier)
            .Where(t => t.SupplierId == supplierId 
                     && t.TransactionDate >= fromDate.Date 
                     && t.TransactionDate <= toDateEnd)
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetBalanceAsOfDateAsync(Guid supplierId, DateTime asOfDate)
    {
        // Get all transactions before the specified date
        var transactions = await _context.SupplierTransactions
            .Where(t => t.SupplierId == supplierId && t.TransactionDate < asOfDate.Date)
            .ToListAsync();
        
        if (!transactions.Any())
            return 0;
        
        // Calculate balance: Credits (purchases) increase balance, Debits (payments/returns) decrease
        return transactions.Sum(t => t.CreditAmount - t.DebitAmount);
    }
}
