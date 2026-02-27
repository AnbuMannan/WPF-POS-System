using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IBillingService
{
    Task<ReceiptDto> CreateSaleAsync(CreateSaleDto dto, string userId, int storeCode, CancellationToken cancellationToken = default);
    Task<string> GenerateBillNumberAsync(CancellationToken cancellationToken = default);
}
