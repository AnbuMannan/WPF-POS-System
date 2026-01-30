using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ITaxProfileService
{
    Task<List<TaxProfileDto>> GetAllAsync(bool includeInactive = false);
    Task<TaxProfileDto> GetByIdAsync(int id);
    Task AddAsync(TaxProfileDto dto);
    Task UpdateAsync(TaxProfileDto dto);
    Task DisableAsync(int id);
}
