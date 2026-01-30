using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync();
    Task<CustomerDto> GetByIdAsync(string id);
    Task AddAsync(CustomerDto customer);
    Task UpdateAsync(CustomerDto customer);
    Task DisableAsync(string id);
    Task<bool> CheckPhoneExistsAsync(string phone, string? excludeId);
}
