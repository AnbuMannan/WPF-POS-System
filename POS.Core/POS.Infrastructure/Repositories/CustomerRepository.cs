using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly PosDbContext _db;

    public CustomerRepository(PosDbContext db) => _db = db;

    public async Task<List<Customer>> GetAllAsync()
        => await _db.Customers
            .AsNoTracking()
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToListAsync();

    public async Task<Customer> GetByIdAsync(string id)
        => await _db.Customers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == id);

    public async Task AddAsync(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        _db.Entry(customer).Property(c => c.CreatedAt).IsModified = false;
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(string id)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
        if (customer == null) return;
        customer.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> CheckPhoneExistsAsync(string phone, string? excludeId)
    {
        var query = _db.Customers.AsNoTracking().Where(c => c.Phone == phone && c.IsActive);
        if (excludeId != null) query = query.Where(c => c.CustomerId != excludeId);
        return await query.AnyAsync();
    }
}
