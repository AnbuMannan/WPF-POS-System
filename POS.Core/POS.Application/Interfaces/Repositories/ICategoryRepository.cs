using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(bool includeInactive = false);
    Task<Category> GetByIdAsync(int id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DisableAsync(int id);
    Task<bool> CheckNameExistsAsync(string name, int? parentCategoryId, int? excludeId);


}
