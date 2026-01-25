using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

namespace POS.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Category>> GetAllAsync()
        => await _repo.GetAllAsync();

    public async Task<Category> GetByIdAsync(Guid id)
        => await _repo.GetByIdAsync(id);

    public async Task AddAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new Exception("Category name is required");

        category.CategoryId = Guid.NewGuid();
        category.IsActive = true;
        await _repo.AddAsync(category);
    }

    public async Task UpdateAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new Exception("Category name is required");

        await _repo.UpdateAsync(category);
    }

    public async Task DisableAsync(Guid id)
        => await _repo.DisableAsync(id);

    public async Task<bool> CheckNameExistsAsync(string name, Guid? parentCategoryId, Guid? excludeId)
    {
        return await _repo.CheckNameExistsAsync(name, parentCategoryId, excludeId);
    }


}
