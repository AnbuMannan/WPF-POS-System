using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.Infrastructure.Services;

public class ReceiptService : IReceiptService
{
    public Task<ReceiptDto> GetReceiptBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ReceiptDto
        {
            SaleId = saleId,
            BillNumber = $"INV{saleId}",
            SaleDate = DateTime.Now,
            TransactionDate = DateTime.Now,
            ReceiptNumber = $"INV{saleId}"
        });
    }
}
