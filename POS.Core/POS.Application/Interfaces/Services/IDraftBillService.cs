using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IDraftBillService
{
    Task<DraftBillDto> SaveDraftAsync(DraftBillDto dto, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DraftBillDto>> GetDraftBillsAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<DraftBillDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
