using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.Infrastructure.Services;

public class HeldBillService : IHeldBillService
{
    public Task<HeldBillDto> HoldBillAsync(HeldBillDto dto, string userId, CancellationToken cancellationToken = default)
    {
        dto.Id = 1;
        dto.HeldAt = DateTime.Now;
        dto.HeldBy = userId;
        return Task.FromResult(dto);
    }

    public Task<IReadOnlyList<HeldBillDto>> GetHeldBillsAsync(string? userId = null, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<HeldBillDto>>(new List<HeldBillDto>());

    public Task<HeldBillDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult<HeldBillDto?>(null);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
