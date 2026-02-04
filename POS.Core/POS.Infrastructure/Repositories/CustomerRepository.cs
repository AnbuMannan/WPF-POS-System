using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly PosDbContext _db;

    public CustomerRepository(PosDbContext db) => _db = db;

    public async Task<List<Customer>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.Customers.AsNoTracking().OrderBy(c => c.Name);
        if (includeInactive)
            query = _db.Customers.IgnoreQueryFilters().AsNoTracking().OrderBy(c => c.Name);
        return await query.ToListAsync();
    }

    public async Task<List<Customer>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await _db.Customers.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        var term = $"%{query.Trim()}%";
        return await _db.Customers
            .AsNoTracking()
            .Where(c => EF.Functions.Like(c.Name, term)
                || (c.Phone != null && EF.Functions.Like(c.Phone, term))
                || (c.Email != null && EF.Functions.Like(c.Email, term)))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
        => await _db.Customers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        var existing = await _db.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == customer.Id);
        if (existing == null)
            throw new InvalidOperationException($"Customer with Id '{customer.Id}' not found.");

        existing.Name = customer.Name;
        existing.Phone = customer.Phone;
        existing.Email = customer.Email;
        existing.Address = customer.Address;
        existing.LoyaltyPoints = customer.LoyaltyPoints;
        existing.IsActive = customer.IsActive;
        existing.UpdatedAt = customer.UpdatedAt ?? DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(Guid id)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer != null)
        {
            customer.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckPhoneExistsAsync(string? phone, Guid? excludeId)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;
        var query = _db.Customers.AsNoTracking().Where(c => c.Phone == phone);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
