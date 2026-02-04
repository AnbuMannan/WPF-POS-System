using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync(bool includeInactive = false);
    Task<List<Customer>> SearchCustomersAsync(string query);
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<Customer> CreateCustomerAsync(CreateCustomerDto dto);
    Task AddAsync(CustomerDto dto);
    Task UpdateAsync(CustomerDto dto);
    Task UpdateLoyaltyPointsAsync(Guid customerId, int points);
    Task DisableAsync(Guid id);
    Task<bool> CheckPhoneExistsAsync(string? phone, Guid? excludeId);
}
