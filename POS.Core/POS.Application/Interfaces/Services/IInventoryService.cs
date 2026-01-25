using POS.Domain.Entities;

namespace POS.Application.Interfaces.Services;

public interface IInventoryService
{
    Task StockInAsync(Guid productId, decimal quantity, string refType, Guid? refId, string remarks);
    Task StockOutAsync(Guid productId, decimal quantity, string refType, Guid? refId, string remarks);
    Task<StockSummary> GetStockAsync(Guid productId);
}
