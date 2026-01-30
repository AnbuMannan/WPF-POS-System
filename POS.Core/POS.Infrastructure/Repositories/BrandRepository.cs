using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly PosDbContext _db;

    public BrandRepository(PosDbContext db) => _db = db;

    public async Task<List<Brand>> GetAllAsync(bool includeInactive = false)
    {
        IQueryable<Brand> query = _db.Brands.AsNoTracking().OrderBy(b => b.Name);
        if (includeInactive)
            query = _db.Brands.IgnoreQueryFilters().AsNoTracking().OrderBy(b => b.Name);
        return await query.ToListAsync();
    }

    public async Task<Brand> GetByIdAsync(int id)
        => await _db.Brands
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BrandId == id);

    public async Task AddAsync(Brand brand)
    {
        _db.Brands.Add(brand);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Brand brand)
    {
        _db.Entry(brand).Property(b => b.CreatedAt).IsModified = false;
        _db.Brands.Update(brand);
        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(int id)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.BrandId == id);
        if (brand == null) return;
        brand.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> CheckNameExistsAsync(string name, int? excludeId)
    {
        var query = _db.Brands.AsNoTracking().Where(b => b.Name == name && b.IsActive);
        if (excludeId != null) query = query.Where(b => b.BrandId != excludeId);
        return await query.AnyAsync();
    }
}
