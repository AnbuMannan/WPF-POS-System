using POS.Domain.Entities;

namespace POS.Application.Interfaces.Services;

public interface ITaxProfileService
{
    Task<List<TaxProfile>> GetAllAsync();
    Task<TaxProfile> GetByIdAsync(Guid id);
    Task AddAsync(TaxProfile taxProfile);
    Task UpdateAsync(TaxProfile taxProfile);
    Task DisableAsync(Guid id);
}
