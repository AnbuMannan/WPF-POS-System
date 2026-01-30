using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;
using AutoMapper;

namespace POS.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> GetAllAsync(bool includeInactive = false)
    {
        var entities = await _repo.GetAllAsync(includeInactive);
        return _mapper.Map<List<CategoryDto>>(entities);
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null! : _mapper.Map<CategoryDto>(entity);
    }

    public async Task AddAsync(CategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Category name is required");

        var category = _mapper.Map<Category>(dto);
        category.IsActive = true;
        await _repo.AddAsync(category);
    }

    public async Task UpdateAsync(CategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Category name is required");

        var category = _mapper.Map<Category>(dto);
        await _repo.UpdateAsync(category);
    }

    public async Task DisableAsync(int id)
        => await _repo.DisableAsync(id);

    public async Task<bool> CheckNameExistsAsync(string name, int? parentCategoryId, int? excludeId)
    {
        return await _repo.CheckNameExistsAsync(name, parentCategoryId, excludeId);
    }
}
