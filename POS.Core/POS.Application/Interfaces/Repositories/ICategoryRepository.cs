using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface TaxCalculator
{
    Task<List<Category>> GetAllAsync();
    Task<Category> GetByIdAsync(Guid id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DisableAsync(Guid id);
    Task<bool> CheckNameExistsAsync(string name, Guid? parentCategoryId, Guid? excludeId);


}
