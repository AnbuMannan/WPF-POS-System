using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface ITaxProfileRepository
{
    Task<List<TaxProfile>> GetAllAsync();
    Task<TaxProfile> GetByIdAsync(Guid id);
    Task AddAsync(TaxProfile taxProfile);
    Task UpdateAsync(TaxProfile taxProfile);
    Task DisableAsync(Guid id);
}
