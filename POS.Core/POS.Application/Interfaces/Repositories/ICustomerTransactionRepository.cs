using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface ICustomerTransactionRepository
{
    Task<List<CustomerTransaction>> GetByCustomerIdAsync(Guid customerId);
    Task<List<CustomerTransaction>> GetByCustomerIdAsync(Guid customerId, DateTime fromDate, DateTime toDate);
    Task<CustomerTransaction> AddAsync(CustomerTransaction transaction);
    Task<decimal> GetBalanceAsync(Guid customerId);
    Task<List<(Customer Customer, decimal Balance, DateTime? LastTransactionDate)>> GetCustomersWithBalanceAsync();
}
