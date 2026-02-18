using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllAsync(bool includeInactive = false);
    Task<SupplierDto?> GetByIdAsync(Guid id);
    Task AddAsync(SupplierDto dto);
    Task UpdateAsync(SupplierDto dto);
    Task DisableAsync(Guid id);
    Task<bool> CheckCodeExistsAsync(string code, Guid? excludeId);
}
