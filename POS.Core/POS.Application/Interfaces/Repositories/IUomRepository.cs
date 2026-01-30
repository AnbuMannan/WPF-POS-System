using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IUomRepository
{
    Task<List<Uom>> GetAllAsync();
    Task<Uom> GetByIdAsync(Guid id);
    Task AddAsync(Uom uom);
    Task UpdateAsync(Uom uom);
    Task DisableAsync(Guid id);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
}
