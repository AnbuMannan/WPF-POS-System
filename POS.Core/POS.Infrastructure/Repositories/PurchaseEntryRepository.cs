using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class PurchaseEntryRepository : IPurchaseEntryRepository
{
    private readonly PosDbContext _db;

    public PurchaseEntryRepository(PosDbContext db) => _db = db;

    public async Task<List<PurchaseEntry>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.PurchaseEntries
            .Include(pe => pe.Supplier)
            .Include(pe => pe.PurchaseOrder)
            .Include(pe => pe.Items)
                .ThenInclude(pei => pei.Product)
            .AsNoTracking()
            .OrderByDescending(pe => pe.ReceivedDate);
        
        if (includeInactive)
            query = _db.PurchaseEntries
                .IgnoreQueryFilters()
                .Include(pe => pe.Supplier)
                .Include(pe => pe.PurchaseOrder)
                .Include(pe => pe.Items)
                    .ThenInclude(pei => pei.Product)
                .AsNoTracking()
                .OrderByDescending(pe => pe.ReceivedDate);
        
        return await query.ToListAsync();
    }

    public async Task<PurchaseEntry?> GetByIdAsync(Guid id, bool includeItems = true)
    {
        var query = _db.PurchaseEntries
            .IgnoreQueryFilters()
            .Include(pe => pe.Supplier)
            .Include(pe => pe.PurchaseOrder)
            .AsNoTracking();
        
        if (includeItems)
        {
            query = query.Include(pe => pe.Items)
                        .ThenInclude(pei => pei.Product);
        }
        
        return await query.FirstOrDefaultAsync(pe => pe.Id == id);
    }

    public async Task<List<PurchaseEntry>> GetBySuppliersAsync(Guid supplierId)
    {
        return await _db.PurchaseEntries
            .Where(pe => pe.SupplierId == supplierId)
            .Include(pe => pe.Supplier)
            .Include(pe => pe.PurchaseOrder)
            .Include(pe => pe.Items)
                .ThenInclude(pei => pei.Product)
            .AsNoTracking()
            .OrderByDescending(pe => pe.ReceivedDate)
            .ToListAsync();
    }

    public async Task<PurchaseEntry?> GetByPurchaseOrderIdAsync(Guid purchaseOrderId)
    {
        return await _db.PurchaseEntries
            .IgnoreQueryFilters()
            .Include(pe => pe.Supplier)
            .Include(pe => pe.PurchaseOrder)
            .Include(pe => pe.Items)
                .ThenInclude(pei => pei.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(pe => pe.PurchaseOrderId == purchaseOrderId);
    }

    public async Task<List<PurchaseEntry>> GetUnprocessedAsync()
    {
        return await _db.PurchaseEntries
            .Where(pe => !pe.IsProcessed)
            .Include(pe => pe.Supplier)
            .Include(pe => pe.PurchaseOrder)
            .Include(pe => pe.Items)
                .ThenInclude(pei => pei.Product)
            .AsNoTracking()
            .OrderByDescending(pe => pe.ReceivedDate)
            .ToListAsync();
    }

    public async Task AddAsync(PurchaseEntry purchaseEntry)
    {
        _db.PurchaseEntries.Add(purchaseEntry);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PurchaseEntry purchaseEntry)
    {
        var existing = await _db.PurchaseEntries
            .IgnoreQueryFilters()
            .Include(pe => pe.Items)
            .FirstOrDefaultAsync(pe => pe.Id == purchaseEntry.Id);
        
        if (existing == null)
            throw new InvalidOperationException($"PurchaseEntry with Id '{purchaseEntry.Id}' not found.");

        existing.SupplierId = purchaseEntry.SupplierId;
        existing.PurchaseOrderId = purchaseEntry.PurchaseOrderId;
        existing.InvoiceNo = purchaseEntry.InvoiceNo;
        existing.InvoiceDate = purchaseEntry.InvoiceDate;
        existing.ReceivedDate = purchaseEntry.ReceivedDate;
        existing.TotalAmount = purchaseEntry.TotalAmount;
        existing.TaxAmount = purchaseEntry.TaxAmount;
        existing.Notes = purchaseEntry.Notes;
        existing.IsProcessed = purchaseEntry.IsProcessed;
        existing.ProcessedAt = purchaseEntry.ProcessedAt;
        existing.ProcessedBy = purchaseEntry.ProcessedBy;
        existing.IsActive = purchaseEntry.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        // Remove existing items
        _db.PurchaseEntryItems.RemoveRange(existing.Items);

        // Add new items
        foreach (var item in purchaseEntry.Items)
        {
            item.PurchaseEntryId = existing.Id;
            _db.PurchaseEntryItems.Add(item);
        }

        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(Guid id)
    {
        var purchaseEntry = await _db.PurchaseEntries.FirstOrDefaultAsync(pe => pe.Id == id);
        if (purchaseEntry != null)
        {
            purchaseEntry.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckInvoiceNoExistsAsync(string invoiceNo, Guid? excludeId)
    {
        if (string.IsNullOrWhiteSpace(invoiceNo))
            return false;
        
        var query = _db.PurchaseEntries.AsNoTracking().Where(pe => pe.InvoiceNo == invoiceNo);
        if (excludeId.HasValue)
            query = query.Where(pe => pe.Id != excludeId.Value);
        
        return await query.AnyAsync();
    }

    /// <summary>
    /// Process a purchase entry and create batches for stock management
    /// This is the CRITICAL logic for market-standard POS system
    /// </summary>
    public async Task ProcessEntryWithInventoryUpdateAsync(Guid purchaseEntryId, bool updateProductPrices)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        
        try
        {
            var entry = await _db.PurchaseEntries
                .Include(pe => pe.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(pe => pe.Id == purchaseEntryId);

            if (entry == null)
                throw new InvalidOperationException($"PurchaseEntry with Id '{purchaseEntryId}' not found.");

            if (entry.IsProcessed)
                throw new InvalidOperationException("This purchase entry has already been processed.");

            // CRITICAL: Create Batches for each Purchase Entry Item
            foreach (var item in entry.Items)
            {
                // 1. Create a new batch record for this item
                var batch = new Batch
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    BatchNo = item.BatchNo ?? $"AUTO-{DateTime.UtcNow:yyyyMMddHHmmss}-{item.ProductId}",
                    ExpiryDate = item.ExpiryDate,
                    ManufactureDate = null, // Can be added later if needed
                    CostPrice = item.CostPrice,
                    SellingPrice = item.SellingPrice,
                    MRP = item.MRP,
                    ReceivedQuantity = item.Quantity,
                    CurrentQuantity = item.Quantity, // Initial quantity = received quantity
                    AllocatedQuantity = 0,
                    SoldQuantity = 0,
                    ReturnedQuantity = 0,
                    AdjustedQuantity = 0,
                    PurchaseEntryId = entry.Id,
                    PurchaseEntryItemId = item.Id,
                    SupplierId = entry.SupplierId,
                    LocationCode = null, // Can be set from warehouse/location
                    BinLocation = null,
                    ReorderLevel = 0,
                    ReceivedDate = entry.ReceivedDate,
                    ReceivedBy = entry.ProcessedBy ?? "System",
                    LastTransactionDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Batches.Add(batch);

                // 2. Update StockSummary (Aggregated Stock)
                var stockSummary = await _db.StockSummaries.FindAsync(item.ProductId);
                if (stockSummary == null)
                {
                    stockSummary = new StockSummary
                    {
                        ProductId = item.ProductId,
                        AvailableStock = item.Quantity,
                        LastUpdated = DateTime.UtcNow
                    };
                    _db.StockSummaries.Add(stockSummary);
                }
                else
                {
                    stockSummary.AvailableStock += item.Quantity;
                    stockSummary.LastUpdated = DateTime.UtcNow;
                }

                // 3. Add StockLedgerEntry (Audit Trail)
                var ledgerEntry = new StockLedgerEntry
                {
                    StockEntryId = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    EntryType = "IN",
                    ReferenceType = "PURCHASE_ENTRY",
                    ReferenceId = entry.Id,
                    EntryDate = DateTime.UtcNow,
                    Remarks = $"Purchase Entry: {entry.InvoiceNo}, Batch: {batch.BatchNo}"
                };
                _db.StockLedgerEntries.Add(ledgerEntry);

                // 4. Update Product Master prices if configured
                if (updateProductPrices)
                {
                    var product = await _db.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.CostPrice = item.CostPrice;
                        product.SellingPrice = item.SellingPrice;
                        product.MRP = item.MRP;
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // 3. Update PurchaseOrder status if linked
            if (entry.PurchaseOrderId.HasValue)
            {
                var purchaseOrder = await _db.PurchaseOrders.FindAsync(entry.PurchaseOrderId.Value);
                if (purchaseOrder != null)
                {
                    purchaseOrder.Status = POS.Domain.Enums.PurchaseOrderStatus.Received;
                    purchaseOrder.UpdatedAt = DateTime.UtcNow;
                }
            }

            // 4. Mark entry as processed
            entry.IsProcessed = true;
            entry.ProcessedAt = DateTime.UtcNow;
            entry.ProcessedBy = "System"; // TODO: Get from auth context

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
