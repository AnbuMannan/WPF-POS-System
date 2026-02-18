using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface ISaleReturnRepository
{
    Task<List<SaleReturn>> GetAllAsync();
    Task<SaleReturn?> GetByIdAsync(int id);
    Task<List<SaleReturn>> GetBySaleIdAsync(long saleId);
    Task<Sale?> GetSaleWithItemsAsync(long saleId);
    Task<Sale?> GetSaleByBillNumberAsync(string billNumber);
    Task<SaleReturn> CreateAsync(SaleReturn saleReturn);
    Task ProcessReturnWithInventoryAsync(int returnId);
    Task<string> GenerateReturnNumberAsync();
    Task<decimal> GetAlreadyReturnedQuantityAsync(long saleItemId);
}
