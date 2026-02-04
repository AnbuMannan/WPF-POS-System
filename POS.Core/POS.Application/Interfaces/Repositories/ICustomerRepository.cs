using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync(bool includeInactive = false);
    Task<List<Customer>> SearchAsync(string query);
    Task<Customer?> GetByIdAsync(Guid id);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DisableAsync(Guid id);
    Task<bool> CheckPhoneExistsAsync(string? phone, Guid? excludeId);
}
