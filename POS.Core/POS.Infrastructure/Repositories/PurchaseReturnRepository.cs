using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class PurchaseReturnRepository : IPurchaseReturnRepository
{
    private readonly PosDbContext _db;

    public PurchaseReturnRepository(PosDbContext db) => _db = db;

    public async Task<List<PurchaseReturn>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.PurchaseReturns
            .Include(pr => pr.Supplier)
            .Include(pr => pr.PurchaseEntry)
            .Include(pr => pr.Items)
                .ThenInclude(pri => pri.Product)
            .AsNoTracking()
            .OrderByDescending(pr => pr.ReturnDate);
        
        if (includeInactive)
            query = _db.PurchaseReturns
                .IgnoreQueryFilters()
                .Include(pr => pr.Supplier)
                .Include(pr => pr.PurchaseEntry)
                .Include(pr => pr.Items)
                    .ThenInclude(pri => pri.Product)
                .AsNoTracking()
                .OrderByDescending(pr => pr.ReturnDate);
        
        return await query.ToListAsync();
    }

    public async Task<PurchaseReturn?> GetByIdAsync(Guid id, bool includeItems = true)
    {
        var query = _db.PurchaseReturns
            .IgnoreQueryFilters()
            .Include(pr => pr.Supplier)
            .Include(pr => pr.PurchaseEntry)
            .AsNoTracking();
        
        if (includeItems)
        {
            query = query.Include(pr => pr.Items)
                        .ThenInclude(pri => pri.Product);
        }
        
        return await query.FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<List<PurchaseReturn>> GetBySupplierAsync(Guid supplierId)
    {
        return await _db.PurchaseReturns
            .Where(pr => pr.SupplierId == supplierId)
            .Include(pr => pr.Supplier)
            .Include(pr => pr.PurchaseEntry)
            .Include(pr => pr.Items)
                .ThenInclude(pri => pri.Product)
            .AsNoTracking()
            .OrderByDescending(pr => pr.ReturnDate)
            .ToListAsync();
    }

    public async Task<List<PurchaseReturn>> GetByPurchaseEntryIdAsync(Guid purchaseEntryId)
    {
        return await _db.PurchaseReturns
            .Where(pr => pr.PurchaseEntryId == purchaseEntryId)
            .Include(pr => pr.Supplier)
            .Include(pr => pr.PurchaseEntry)
            .Include(pr => pr.Items)
                .ThenInclude(pri => pri.Product)
            .AsNoTracking()
            .OrderByDescending(pr => pr.ReturnDate)
            .ToListAsync();
    }

    public async Task<List<PurchaseReturn>> GetUnprocessedAsync()
    {
        return await _db.PurchaseReturns
            .Where(pr => !pr.IsProcessed)
            .Include(pr => pr.Supplier)
            .Include(pr => pr.PurchaseEntry)
            .Include(pr => pr.Items)
                .ThenInclude(pri => pri.Product)
            .AsNoTracking()
            .OrderByDescending(pr => pr.ReturnDate)
            .ToListAsync();
    }

    public async Task AddAsync(PurchaseReturn purchaseReturn)
    {
        _db.PurchaseReturns.Add(purchaseReturn);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PurchaseReturn purchaseReturn)
    {
        var existing = await _db.PurchaseReturns
            .IgnoreQueryFilters()
            .Include(pr => pr.Items)
            .FirstOrDefaultAsync(pr => pr.Id == purchaseReturn.Id);
        
        if (existing == null)
            throw new InvalidOperationException($"PurchaseReturn with Id '{purchaseReturn.Id}' not found.");

        existing.SupplierId = purchaseReturn.SupplierId;
        existing.PurchaseEntryId = purchaseReturn.PurchaseEntryId;
        existing.ReturnNo = purchaseReturn.ReturnNo;
        existing.ReturnDate = purchaseReturn.ReturnDate;
        existing.TotalAmount = purchaseReturn.TotalAmount;
        existing.TaxAmount = purchaseReturn.TaxAmount;
        existing.Reason = purchaseReturn.Reason;
        existing.Notes = purchaseReturn.Notes;
        existing.Status = purchaseReturn.Status;
        existing.IsProcessed = purchaseReturn.IsProcessed;
        existing.ProcessedAt = purchaseReturn.ProcessedAt;
        existing.ProcessedBy = purchaseReturn.ProcessedBy;
        existing.IsActive = purchaseReturn.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        // Remove existing items
        _db.PurchaseReturnItems.RemoveRange(existing.Items);

        // Add new items
        foreach (var item in purchaseReturn.Items)
        {
            item.PurchaseReturnId = existing.Id;
            _db.PurchaseReturnItems.Add(item);
        }

        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(Guid id)
    {
        var purchaseReturn = await _db.PurchaseReturns.FirstOrDefaultAsync(pr => pr.Id == id);
        if (purchaseReturn != null)
        {
            purchaseReturn.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckReturnNoExistsAsync(string returnNo, Guid? excludeId)
    {
        if (string.IsNullOrWhiteSpace(returnNo))
            return false;
        
        var query = _db.PurchaseReturns.AsNoTracking().Where(pr => pr.ReturnNo == returnNo);
        if (excludeId.HasValue)
            query = query.Where(pr => pr.Id != excludeId.Value);
        
        return await query.AnyAsync();
    }

    /// <summary>
    /// Process a purchase return and reduce stock from batches
    /// CRITICAL logic for market-standard POS system
    /// </summary>
    public async Task ProcessReturnWithInventoryUpdateAsync(Guid purchaseReturnId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        
        try
        {
            var returnEntry = await _db.PurchaseReturns
                .Include(pr => pr.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(pr => pr.Id == purchaseReturnId);

            if (returnEntry == null)
                throw new InvalidOperationException($"PurchaseReturn with Id '{purchaseReturnId}' not found.");

            if (returnEntry.IsProcessed)
                throw new InvalidOperationException("This purchase return has already been processed.");

            // CRITICAL: Update stock for each returned item
            foreach (var item in returnEntry.Items)
            {
                // 1. Find the appropriate batch to reduce stock from
                // Priority: Match by batch number if provided, otherwise use FIFO (oldest batch first)
                Batch? batch = null;

                if (!string.IsNullOrWhiteSpace(item.BatchNo))
                {
                    // Try to find the specific batch
                    batch = await _db.Batches
                        .Where(b => b.ProductId == item.ProductId && b.BatchNo == item.BatchNo && b.IsActive)
                        .FirstOrDefaultAsync();
                }
                
                if (batch == null)
                {
                    // Fall back to FIFO - oldest batch with available stock
                    batch = await _db.Batches
                        .Where(b => b.ProductId == item.ProductId && b.CurrentQuantity > 0 && b.IsActive)
                        .OrderBy(b => b.ReceivedDate)
                        .FirstOrDefaultAsync();
                }

                if (batch == null)
                {
                    throw new InvalidOperationException($"No available batch found for product {item.ProductId} to process return.");
                }

                // Validate sufficient stock
                if (batch.CurrentQuantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock in batch {batch.BatchNo} for product {item.ProductId}. " +
                        $"Available: {batch.CurrentQuantity}, Return Qty: {item.Quantity}");
                }

                // 2. Reduce stock from batch
                batch.CurrentQuantity -= item.Quantity;
                batch.ReturnedQuantity += item.Quantity;
                batch.LastTransactionDate = DateTime.UtcNow;
                batch.UpdatedAt = DateTime.UtcNow;

                // 3. Create Stock Ledger Entry for audit trail
                var ledgerEntry = new StockLedgerEntry
                {
                    StockEntryId = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = -item.Quantity, // Negative for stock reduction
                    EntryType = "OUT",
                    ReferenceType = "PURCHASE_RETURN",
                    ReferenceId = returnEntry.Id,
                    EntryDate = DateTime.UtcNow,
                    Remarks = $"Purchase Return: {returnEntry.ReturnNo} - Batch: {batch.BatchNo}"
                };

                _db.StockLedgerEntries.Add(ledgerEntry);
            }

            // 4. Mark return as processed
            returnEntry.IsProcessed = true;
            returnEntry.ProcessedAt = DateTime.UtcNow;
            returnEntry.ProcessedBy = "System"; // TODO: Get from auth context
            returnEntry.Status = "Processed";

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
