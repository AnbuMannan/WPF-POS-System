using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;
using POS.Shared.Models;

namespace POS.Infrastructure.Repositories;

public class StockAdjustmentRepository : IStockAdjustmentRepository
{
    private readonly PosDbContext _context;
    private readonly ILogger<StockAdjustmentRepository> _logger;

    public StockAdjustmentRepository(PosDbContext context, ILogger<StockAdjustmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<StockAdjustment>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.StockAdjustments
            .Include(a => a.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(a => a.IsActive);

        return await query
            .OrderByDescending(a => a.AdjustmentDate)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<StockAdjustment?> GetByIdAsync(Guid id, bool includeItems = false)
    {
        var query = _context.StockAdjustments.AsQueryable();

        if (includeItems)
        {
            query = query
                .Include(a => a.Items)
                .ThenInclude(i => i.Product);
        }

        return await query.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<StockAdjustment?> GetByReferenceNoAsync(string referenceNo)
    {
        return await _context.StockAdjustments
            .Include(a => a.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(a => a.ReferenceNo == referenceNo);
    }

    public async Task<IEnumerable<StockAdjustment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        var toDateEnd = toDate.Date.AddDays(1).AddTicks(-1);
        
        return await _context.StockAdjustments
            .Include(a => a.Items)
            .Where(a => a.IsActive && a.AdjustmentDate >= fromDate.Date && a.AdjustmentDate <= toDateEnd)
            .OrderByDescending(a => a.AdjustmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockAdjustment>> GetByReasonAsync(string reason)
    {
        return await _context.StockAdjustments
            .Include(a => a.Items)
            .Where(a => a.IsActive && a.Reason == reason)
            .OrderByDescending(a => a.AdjustmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockAdjustment>> GetByStatusAsync(string status)
    {
        return await _context.StockAdjustments
            .Include(a => a.Items)
            .Where(a => a.IsActive && a.Status == status)
            .OrderByDescending(a => a.AdjustmentDate)
            .ToListAsync();
    }

    public async Task<string> GenerateReferenceNoAsync()
    {
        var year = DateTime.Now.Year;
        var prefix = $"ADJ-{year}-";

        var lastRef = await _context.StockAdjustments
            .Where(a => a.ReferenceNo.StartsWith(prefix))
            .OrderByDescending(a => a.ReferenceNo)
            .Select(a => a.ReferenceNo)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastRef))
        {
            var numPart = lastRef.Replace(prefix, "");
            if (int.TryParse(numPart, out int lastNum))
            {
                nextNumber = lastNum + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    public async Task<StockAdjustment> CreateAsync(StockAdjustment adjustment)
    {
        adjustment.CreatedAt = DateTime.Now;
        adjustment.IsActive = true;

        foreach (var item in adjustment.Items)
        {
            item.Id = Guid.NewGuid();
            item.CreatedAt = DateTime.Now;
            item.IsActive = true;
        }

        _context.StockAdjustments.Add(adjustment);
        await _context.SaveChangesAsync();
        return adjustment;
    }

    public async Task<StockAdjustment> UpdateAsync(StockAdjustment adjustment)
    {
        adjustment.UpdatedAt = DateTime.Now;
        _context.StockAdjustments.Update(adjustment);
        await _context.SaveChangesAsync();
        return adjustment;
    }

    public async Task ApproveAsync(Guid id, string approvedBy)
    {
        var adjustment = await GetByIdAsync(id, includeItems: true);
        if (adjustment == null)
            throw new InvalidOperationException($"StockAdjustment with Id '{id}' not found.");

        adjustment.Status = AdjustmentStatus.Approved;
        adjustment.ApprovedAt = DateTime.Now;
        adjustment.ApprovedBy = approvedBy;
        adjustment.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid id)
    {
        var adjustment = await GetByIdAsync(id);
        if (adjustment == null)
            throw new InvalidOperationException($"StockAdjustment with Id '{id}' not found.");

        adjustment.Status = AdjustmentStatus.Cancelled;
        adjustment.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task DisableAsync(Guid id)
    {
        var adjustment = await GetByIdAsync(id);
        if (adjustment == null)
            throw new InvalidOperationException($"StockAdjustment with Id '{id}' not found.");

        adjustment.IsActive = false;
        adjustment.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// CRITICAL: Process adjustment with inventory updates in a transaction
    /// </summary>
    public async Task ProcessAdjustmentWithInventoryAsync(Guid id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var adjustment = await GetByIdAsync(id, includeItems: true);
            if (adjustment == null)
                throw new InvalidOperationException($"StockAdjustment with Id '{id}' not found.");

            if (adjustment.Status == AdjustmentStatus.Approved)
                throw new InvalidOperationException("This adjustment has already been processed.");

            foreach (var item in adjustment.Items)
            {
                // Update Batch stock (primary stock tracking)
                // If a specific batch is mentioned, update that batch
                // Otherwise, update the latest batch or create a new one
                if (!string.IsNullOrEmpty(item.BatchNo))
                {
                    var batch = await _context.Batches
                        .FirstOrDefaultAsync(b => b.ProductId == item.ProductId 
                                                && b.BatchNo == item.BatchNo 
                                                && b.IsActive);
                    
                    if (batch != null)
                    {
                        batch.CurrentQuantity += item.Quantity;
                        batch.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        // Create new batch for positive adjustments
                        if (item.Quantity > 0)
                        {
                            var defaultSupplier = await _context.Suppliers
                                .FirstOrDefaultAsync(s => s.Code == "SYS-SUPPLIER" || s.Name == "System Default");

                            if (defaultSupplier == null)
                            {
                                defaultSupplier = new Supplier
                                {
                                    Id = Guid.NewGuid(),
                                    Name = "System Default",
                                    Code = "SYS-SUPPLIER",
                                    ContactPerson = "System",
                                    IsActive = true,
                                    CreatedAt = DateTime.Now
                                };
                                _context.Suppliers.Add(defaultSupplier);
                            }

                            var newBatch = new Batch
                            {
                                Id = Guid.NewGuid(),
                                ProductId = item.ProductId,
                                BatchNo = item.BatchNo,
                                ReceivedQuantity = item.Quantity,
                                CurrentQuantity = item.Quantity,
                                CostPrice = item.CostPrice,
                                SellingPrice = item.CostPrice * 1.2m, // 20% markup default
                                ReceivedDate = DateTime.Now,
                                SupplierId = defaultSupplier.Id,
                                IsActive = true,
                                CreatedAt = DateTime.Now
                            };
                            _context.Batches.Add(newBatch);
                        }
                    }
                }
                else
                {
                    // No specific batch - find the latest batch or create a new one
                    var latestBatch = await _context.Batches
                        .Where(b => b.ProductId == item.ProductId && b.IsActive)
                        .OrderByDescending(b => b.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (latestBatch != null)
                    {
                        latestBatch.CurrentQuantity += item.Quantity;
                        latestBatch.UpdatedAt = DateTime.Now;
                    }
                    else if (item.Quantity > 0)
                    {
                        // Handle default supplier for Grade A market standard
                        var defaultSupplier = await _context.Suppliers
                            .FirstOrDefaultAsync(s => s.Code == "SYS-SUPPLIER" || s.Name == "System Default");

                        if (defaultSupplier == null)
                        {
                            _logger.LogInformation("Creating System Default Supplier for stock adjustments");
                            defaultSupplier = new Supplier
                            {
                                Id = Guid.NewGuid(),
                                Name = "System Default",
                                Code = "SYS-SUPPLIER",
                                ContactPerson = "System",
                                IsActive = true,
                                CreatedAt = DateTime.Now
                            };
                            _context.Suppliers.Add(defaultSupplier);
                            // We don't save yet, it will save with the adjustment
                        }

                        // Create an adjustment batch for positive adjustments
                        var adjBatch = new Batch
                        {
                            Id = Guid.NewGuid(),
                            ProductId = item.ProductId,
                            BatchNo = $"ADJ-{DateTime.Now:yyyyMMddHHmmss}",
                            ReceivedQuantity = item.Quantity,
                            CurrentQuantity = item.Quantity,
                            CostPrice = item.CostPrice,
                            SellingPrice = item.CostPrice * 1.2m,
                            ReceivedDate = DateTime.Now,
                            SupplierId = defaultSupplier.Id,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };
                        _context.Batches.Add(adjBatch);
                    }
                }

                // Create StockLedgerEntry
                var ledgerEntry = new StockLedgerEntry
                {
                    StockEntryId = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = Math.Abs(item.Quantity),
                    EntryType = item.Quantity >= 0 ? "IN" : "OUT",
                    ReferenceType = "Adjustment",
                    ReferenceId = adjustment.Id,
                    EntryDate = DateTime.Now,
                    Remarks = $"{adjustment.Reason}: {item.Remarks ?? adjustment.Remarks ?? ""}"
                };
                _context.Set<StockLedgerEntry>().Add(ledgerEntry);
            }

            // Mark as approved
            adjustment.Status = AdjustmentStatus.Approved;
            adjustment.ApprovedAt = DateTime.Now;
            adjustment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation("Stock adjustment {ReferenceNo} processed successfully", adjustment.ReferenceNo);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            
            // Re-fetch adjustment without using the potentially corrupted context state if necessary, 
            // but here we just need the ID if the variable is lost (which it isn't, but let's be safe)
            _logger.LogError(ex, "Error processing stock adjustment with ID {AdjustmentId}. Details: {Message}", 
                id, ex.InnerException?.Message ?? ex.Message);
            throw;
        }
    }
}
