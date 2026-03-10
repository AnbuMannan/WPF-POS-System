using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class UomRepository : IUomRepository
{
    private readonly PosDbContext _db;

    public UomRepository(PosDbContext db)
    {
        _db = db;
    }

    public async Task<List<Uom>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.Uoms.AsNoTracking().OrderBy(u => u.Name);
        if (includeInactive)
            query = _db.Uoms.IgnoreQueryFilters().AsNoTracking().OrderBy(u => u.Name);
        return await query.ToListAsync();
    }

    public async Task<Uom> GetByIdAsync(Guid id)
        => await _db.Uoms
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task AddAsync(Uom uom)
    {
        _db.Uoms.Add(uom);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Uom uom)
    {
        var existing = await _db.Uoms.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == uom.Id);
        if (existing == null)
            throw new InvalidOperationException($"UoM with Id '{uom.Id}' not found.");

        existing.Name = uom.Name;
        existing.Code = uom.Code;
        existing.Symbol = uom.Symbol;
        existing.DecimalPlaces = uom.DecimalPlaces;
        existing.Description = uom.Description;
        existing.IsActive = uom.IsActive;
        existing.UpdatedAt = uom.UpdatedAt ?? DateTime.Now;

        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(Guid id)
    {
        var uom = await _db.Uoms.FirstOrDefaultAsync(u => u.Id == id);
        if (uom != null)
        {
            uom.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
    {
        var query = _db.Uoms.AsNoTracking().Where(u => u.Code == code);
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
