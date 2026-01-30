using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IBrandRepository
{
    Task<List<Brand>> GetAllAsync(bool includeInactive = false);
    Task<Brand> GetByIdAsync(int id);
    Task AddAsync(Brand brand);
    Task UpdateAsync(Brand brand);
    Task DisableAsync(int id);
    Task<bool> CheckNameExistsAsync(string name, int? excludeId);
}
