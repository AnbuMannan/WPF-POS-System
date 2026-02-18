using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class CustomerTransactionRepository : ICustomerTransactionRepository
{
    private readonly PosDbContext _context;

    public CustomerTransactionRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerTransaction>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.CustomerTransactions
            .Where(ct => ct.CustomerId == customerId)
            .OrderByDescending(ct => ct.TransactionDate)
            .ToListAsync();
    }

    public async Task<List<CustomerTransaction>> GetByCustomerIdAsync(Guid customerId, DateTime fromDate, DateTime toDate)
    {
        return await _context.CustomerTransactions
            .Where(ct => ct.CustomerId == customerId
                      && ct.TransactionDate >= fromDate
                      && ct.TransactionDate <= toDate)
            .OrderByDescending(ct => ct.TransactionDate)
            .ToListAsync();
    }

    public async Task<CustomerTransaction> AddAsync(CustomerTransaction transaction)
    {
        _context.CustomerTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<decimal> GetBalanceAsync(Guid customerId)
    {
        var lastTransaction = await _context.CustomerTransactions
            .Where(ct => ct.CustomerId == customerId)
            .OrderByDescending(ct => ct.TransactionDate)
            .ThenByDescending(ct => ct.CreatedAt)
            .FirstOrDefaultAsync();

        return lastTransaction?.Balance ?? 0;
    }

    public async Task<List<(Customer Customer, decimal Balance, DateTime? LastTransactionDate)>> GetCustomersWithBalanceAsync()
    {
        var customers = await _context.Customers.ToListAsync();
        var result = new List<(Customer, decimal, DateTime?)>();

        foreach (var customer in customers)
        {
            var lastTransaction = await _context.CustomerTransactions
                .Where(ct => ct.CustomerId == customer.Id)
                .OrderByDescending(ct => ct.TransactionDate)
                .ThenByDescending(ct => ct.CreatedAt)
                .FirstOrDefaultAsync();

            var balance = lastTransaction?.Balance ?? 0;
            var lastDate = lastTransaction?.TransactionDate;

            result.Add((customer, balance, lastDate));
        }

        return result;
    }
}
