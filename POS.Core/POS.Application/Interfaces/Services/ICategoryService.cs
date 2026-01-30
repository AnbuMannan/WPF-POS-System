using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(bool includeInactive = false);
    Task<CategoryDto> GetByIdAsync(int id);
    Task AddAsync(CategoryDto category);
    Task UpdateAsync(CategoryDto category);
    Task DisableAsync(int id);
    Task<bool> CheckNameExistsAsync(string name, int? parentCategoryId, int? excludeId);


}
