using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly PosDbContext _db;

    public CategoryRepository(PosDbContext db) => _db = db;

    public async Task<List<Category>> GetAllAsync(bool includeInactive = false)
    {
        IQueryable<Category> query = _db.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder);
        if (includeInactive)
            query = _db.Categories
                .IgnoreQueryFilters()
                .Include(c => c.ParentCategory)
                .AsNoTracking()
                .OrderBy(c => c.DisplayOrder);
        return await query.ToListAsync();
    }

    public async Task<Category> GetByIdAsync(int id)
        => await _db.Categories
            .Include(c => c.ParentCategory)
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id);

    public async Task AddAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _db.Entry(category).Property(c => c.CreatedAt).IsModified = false;
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(int id)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category == null) return;
        category.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> CheckNameExistsAsync(string name, int? parentCategoryId, int? excludeId)
    {
        var query = _db.Categories.AsNoTracking().Where(c => c.Name == name && c.IsActive);
        if (parentCategoryId == null)
            query = query.Where(c => c.ParentCategoryId == null);
        else
            query = query.Where(c => c.ParentCategoryId == parentCategoryId);
        if (excludeId != null)
            query = query.Where(c => c.CategoryId != excludeId);
        return await query.AnyAsync();
    }
}
