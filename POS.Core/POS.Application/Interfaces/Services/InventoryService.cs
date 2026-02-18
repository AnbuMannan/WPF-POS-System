using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

namespace POS.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repo;
    private readonly IAuditLogService _auditLogService;

    public InventoryService(IInventoryRepository repo, IAuditLogService auditLogService)
    {
        _repo = repo;
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
    }

    public async Task StockInAsync(long productId, decimal quantity, string refType, Guid? refId, string remarks)
    {
        await AddEntry(productId, quantity, "IN", refType, refId, remarks);
        await _auditLogService.LogAsync("API", "InventoryAdjustment", "StockLedger", productId.ToString(), null, $"IN qty={quantity} refType={refType} remarks={remarks}");
    }

    public async Task StockOutAsync(long productId, decimal quantity, string refType, Guid? refId, string remarks)
    {
        await AddEntry(productId, -quantity, "OUT", refType, refId, remarks);
        await _auditLogService.LogAsync("API", "InventoryAdjustment", "StockLedger", productId.ToString(), null, $"OUT qty={quantity} refType={refType} remarks={remarks}");
    }

    private async Task AddEntry(long productId, decimal qty, string entryType, string refType, Guid? refId, string remarks)
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

    public async Task<StockSummary> GetStockAsync(long productId)
        => await _repo.GetStockAsync(productId);
}
