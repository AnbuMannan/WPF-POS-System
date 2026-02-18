using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface ISupplierRepository
{
    Task<List<Supplier>> GetAllAsync(bool includeInactive = false);
    Task<Supplier?> GetByIdAsync(Guid id);
    Task AddAsync(Supplier supplier);
    Task UpdateAsync(Supplier supplier);
    Task DisableAsync(Guid id);
    Task<bool> CheckCodeExistsAsync(string code, Guid? excludeId);
}
