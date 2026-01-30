using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IBrandService
{
    Task<List<BrandDto>> GetAllAsync(bool includeInactive = false);
    Task<BrandDto> GetByIdAsync(int id);
    Task AddAsync(BrandDto brand);
    Task UpdateAsync(BrandDto brand);
    Task DisableAsync(int id);
    Task<bool> CheckNameExistsAsync(string name, int? excludeId);
}
