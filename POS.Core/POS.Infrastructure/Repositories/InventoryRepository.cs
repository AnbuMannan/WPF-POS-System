using Dapper;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using System.Data;

namespace POS.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDbConnection _db;

    public InventoryRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task AddLedgerEntryAsync(StockLedgerEntry entry)
    {
        await _db.ExecuteAsync(@"
        INSERT INTO StockLedgerEntries
        (StockEntryId,ProductId,Quantity,EntryType,ReferenceType,ReferenceId,EntryDate,Remarks)
        VALUES
        (@StockEntryId,@ProductId,@Quantity,@EntryType,@ReferenceType,@ReferenceId,@EntryDate,@Remarks)", entry);
    }

    public async Task UpdateStockAsync(long productId, decimal delta)
    {
        await _db.ExecuteAsync(@"
        INSERT INTO StockSummary(ProductId,AvailableStock,LastUpdated)
        VALUES(@ProductId,@Delta,NOW())
        ON DUPLICATE KEY UPDATE 
        AvailableStock = AvailableStock + @Delta,
        LastUpdated = NOW()", new { ProductId = productId, Delta = delta });
    }

    public async Task<StockSummary> GetStockAsync(long productId)
    {
        return await _db.QueryFirstOrDefaultAsync<StockSummary>(
            "SELECT * FROM StockSummary WHERE ProductId=@productId",
            new { productId });
    }
}
