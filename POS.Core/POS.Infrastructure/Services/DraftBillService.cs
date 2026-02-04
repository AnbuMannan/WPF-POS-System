using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.Infrastructure.Services;

public class DraftBillService : IDraftBillService
{
    public Task<DraftBillDto> SaveDraftAsync(DraftBillDto dto, string userId, CancellationToken cancellationToken = default)
    {
        dto.Id = 1;
        dto.CreatedAt = DateTime.Now;
        dto.CreatedBy = userId;
        return Task.FromResult(dto);
    }

    public Task<IReadOnlyList<DraftBillDto>> GetDraftBillsAsync(string? userId = null, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DraftBillDto>>(new List<DraftBillDto>());

    public Task<DraftBillDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult<DraftBillDto?>(null);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
