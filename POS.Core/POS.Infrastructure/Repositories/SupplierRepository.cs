using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly PosDbContext _db;

    public SupplierRepository(PosDbContext db) => _db = db;

    public async Task<List<Supplier>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.Suppliers.AsNoTracking().OrderBy(s => s.Name);
        if (includeInactive)
            query = _db.Suppliers.IgnoreQueryFilters().AsNoTracking().OrderBy(s => s.Name);
        return await query.ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(Guid id)
        => await _db.Suppliers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task AddAsync(Supplier supplier)
    {
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        var existing = await _db.Suppliers.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == supplier.Id);
        if (existing == null)
            throw new InvalidOperationException($"Supplier with Id '{supplier.Id}' not found.");

        existing.Name = supplier.Name;
        existing.Code = supplier.Code;
        existing.ContactPerson = supplier.ContactPerson;
        existing.Mobile = supplier.Mobile;
        existing.Email = supplier.Email;
        existing.Address = supplier.Address;
        existing.GstVatNumber = supplier.GstVatNumber;
        existing.CreditPeriodDays = supplier.CreditPeriodDays;
        existing.CreditLimit = supplier.CreditLimit;
        existing.IsActive = supplier.IsActive;
        existing.UpdatedAt = supplier.UpdatedAt ?? DateTime.Now;

        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(Guid id)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
        if (supplier != null)
        {
            supplier.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckCodeExistsAsync(string code, Guid? excludeId)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;
        var query = _db.Suppliers.AsNoTracking().Where(s => s.Code == code);
        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
