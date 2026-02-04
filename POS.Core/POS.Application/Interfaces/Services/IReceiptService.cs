using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IReceiptService
{
    Task<ReceiptDto> GetReceiptBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);
}
