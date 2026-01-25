using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

namespace POS.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repo;

    public InventoryService(IInventoryRepository repo)
    {
        _repo = repo;
    }

    public async Task StockInAsync(Guid productId, decimal quantity, string refType, Guid? refId, string remarks)
    {
        await AddEntry(productId, quantity, "IN", refType, refId, remarks);
    }

    public async Task StockOutAsync(Guid productId, decimal quantity, string refType, Guid? refId, string remarks)
    {
        await AddEntry(productId, -quantity, "OUT", refType, refId, remarks);
    }

    private async Task AddEntry(Guid productId, decimal qty, string entryType, string refType, Guid? refId, string remarks)
    {
        var entry = new StockLedgerEntry
        {
            StockEntryId = Guid.NewGuid(),
            ProductId = productId,
            Quantity = qty,
            EntryType = entryType,
            ReferenceType = refType,
            ReferenceId = refId,
            EntryDate = DateTime.UtcNow,
            Remarks = remarks
        };

        await _repo.AddLedgerEntryAsync(entry);
        await _repo.UpdateStockAsync(productId, qty);
    }

    public async Task<StockSummary> GetStockAsync(Guid productId)
        => await _repo.GetStockAsync(productId);
}
