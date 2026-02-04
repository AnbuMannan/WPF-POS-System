using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IHeldBillService
{
    Task<HeldBillDto> HoldBillAsync(HeldBillDto dto, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HeldBillDto>> GetHeldBillsAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<HeldBillDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
