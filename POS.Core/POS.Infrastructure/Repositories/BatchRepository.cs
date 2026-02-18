using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class BatchRepository : IBatchRepository
{
    private readonly PosDbContext _db;

    public BatchRepository(PosDbContext db) => _db = db;

    public async Task<List<Batch>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.Batches
            .Include(b => b.Product)
            .Include(b => b.Supplier)
            .AsNoTracking()
            .OrderByDescending(b => b.ReceivedDate);
        
        if (includeInactive)
            query = _db.Batches
                .IgnoreQueryFilters()
                .Include(b => b.Product)
                .Include(b => b.Supplier)
                .AsNoTracking()
                .OrderByDescending(b => b.ReceivedDate);
        
        return await query.ToListAsync();
    }

    public async Task<Batch?> GetByIdAsync(Guid id)
    {
        return await _db.Batches
            .IgnoreQueryFilters()
            .Include(b => b.Product)
            .Include(b => b.Supplier)
            .Include(b => b.PurchaseEntry)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Batch>> GetByProductIdAsync(long productId)
    {
        return await _db.Batches
            .Where(b => b.ProductId == productId)
            .Include(b => b.Supplier)
            .AsNoTracking()
            .OrderBy(b => b.ExpiryDate)
            .ThenByDescending(b => b.ReceivedDate)
            .ToListAsync();
    }

    public async Task<List<Batch>> GetByBatchNoAsync(string batchNo)
    {
        return await _db.Batches
            .Where(b => b.BatchNo == batchNo)
            .Include(b => b.Product)
            .Include(b => b.Supplier)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Batch>> GetAvailableBatchesAsync(long productId)
    {
        return await _db.Batches
            .Where(b => b.ProductId == productId && b.CurrentQuantity > 0)
            .Include(b => b.Product)
            .AsNoTracking()
            .OrderBy(b => b.ExpiryDate)
            .ThenBy(b => b.ReceivedDate) // FIFO
            .ToListAsync();
    }

    public async Task<List<Batch>> GetExpiredBatchesAsync()
    {
        var today = DateTime.Today;
        return await _db.Batches
            .Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value < today && b.CurrentQuantity > 0)
            .Include(b => b.Product)
            .AsNoTracking()
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    public async Task<List<Batch>> GetExpiringBatchesAsync(int daysThreshold = 30)
    {
        var futureDate = DateTime.Today.AddDays(daysThreshold);
        return await _db.Batches
            .Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value <= futureDate && b.CurrentQuantity > 0)
            .Include(b => b.Product)
            .AsNoTracking()
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    public async Task<Batch?> GetByPurchaseEntryItemAsync(Guid purchaseEntryItemId)
    {
        return await _db.Batches
            .FirstOrDefaultAsync(b => b.PurchaseEntryItemId == purchaseEntryItemId);
    }

    public async Task AddAsync(Batch batch)
    {
        _db.Batches.Add(batch);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Batch batch)
    {
        var existing = await _db.Batches
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == batch.Id);
        
        if (existing == null)
            throw new InvalidOperationException($"Batch with Id '{batch.Id}' not found.");

        existing.BatchNo = batch.BatchNo;
        existing.ExpiryDate = batch.ExpiryDate;
        existing.ManufactureDate = batch.ManufactureDate;
        existing.CostPrice = batch.CostPrice;
        existing.SellingPrice = batch.SellingPrice;
        existing.MRP = batch.MRP;
        existing.CurrentQuantity = batch.CurrentQuantity;
        existing.AllocatedQuantity = batch.AllocatedQuantity;
        existing.SoldQuantity = batch.SoldQuantity;
        existing.ReturnedQuantity = batch.ReturnedQuantity;
        existing.AdjustedQuantity = batch.AdjustedQuantity;
        existing.LocationCode = batch.LocationCode;
        existing.BinLocation = batch.BinLocation;
        existing.ReorderLevel = batch.ReorderLevel;
        existing.LastTransactionDate = batch.LastTransactionDate;
        existing.IsActive = batch.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(Guid id)
    {
        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == id);
        if (batch != null)
        {
            batch.IsActive = false;
            batch.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalStockForProductAsync(long productId)
    {
        return await _db.Batches
            .Where(b => b.ProductId == productId)
            .SumAsync(b => b.CurrentQuantity);
    }

    public async Task<decimal> GetAvailableStockForProductAsync(long productId)
    {
        return await _db.Batches
            .Where(b => b.ProductId == productId)
            .SumAsync(b => b.CurrentQuantity - b.AllocatedQuantity);
    }
}
