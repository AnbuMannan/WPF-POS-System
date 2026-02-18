using POS.Domain.Entities;

namespace POS.Application.Interfaces.Services;

public interface IInventoryService
{
    Task StockInAsync(long productId, decimal quantity, string refType, Guid? refId, string remarks);
    Task StockOutAsync(long productId, decimal quantity, string refType, Guid? refId, string remarks);
    Task<StockSummary> GetStockAsync(long productId);
}
