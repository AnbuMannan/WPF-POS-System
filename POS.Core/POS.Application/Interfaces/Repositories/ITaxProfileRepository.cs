using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface ITaxProfileRepository
{
    Task<List<TaxProfile>> GetAllAsync(bool includeInactive = false);
    Task<TaxProfile> GetByIdAsync(int id);
    Task AddAsync(TaxProfile taxProfile);
    Task UpdateAsync(TaxProfile taxProfile);
    Task DisableAsync(int id);
}
