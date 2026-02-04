using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IUomService
{
    Task<List<UomDto>> GetAllAsync(bool includeInactive = false);
    Task<UomDto?> GetByIdAsync(Guid id);
    Task AddAsync(UomDto dto);
    Task UpdateAsync(UomDto dto);
    Task DisableAsync(Guid id);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId);
}
