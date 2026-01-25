using POS.Domain.Entities;

namespace POS.Application.Interfaces.Services;

public interface IBrandService
{
    Task<List<Brand>> GetAllAsync();
    Task<Brand> GetByIdAsync(Guid id);
    Task AddAsync(Brand brand);
    Task UpdateAsync(Brand brand);
    Task DisableAsync(Guid id);
}
