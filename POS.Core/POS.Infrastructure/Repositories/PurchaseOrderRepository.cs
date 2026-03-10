using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly PosDbContext _db;

    public PurchaseOrderRepository(PosDbContext db) => _db = db;

    public async Task<List<PurchaseOrder>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
                .ThenInclude(poi => poi.Product)
            .AsNoTracking()
            .OrderByDescending(po => po.OrderDate);
        
        if (includeInactive)
            query = _db.PurchaseOrders
                .IgnoreQueryFilters()
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                    .ThenInclude(poi => poi.Product)
                .AsNoTracking()
                .OrderByDescending(po => po.OrderDate);
        
        return await query.ToListAsync();
    }

    public async Task<PurchaseOrder?> GetByIdAsync(Guid id, bool includeItems = true)
    {
        var query = _db.PurchaseOrders
            .IgnoreQueryFilters()
            .Include(po => po.Supplier)
            .AsNoTracking();
        
        if (includeItems)
        {
            query = query.Include(po => po.Items)
                        .ThenInclude(poi => poi.Product);
        }
        
        return await query.FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<List<PurchaseOrder>> GetPendingOrdersBySuppliersAsync(Guid supplierId)
    {
        // Return both Draft and Pending status POs that can be received in GRN
        return await _db.PurchaseOrders
            .Where(po => po.SupplierId == supplierId && 
                   (po.Status == PurchaseOrderStatus.Draft || po.Status == PurchaseOrderStatus.Pending))
            .Include(po => po.Supplier)
            .Include(po => po.Items)
                .ThenInclude(poi => poi.Product)
            .AsNoTracking()
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync();
    }

    public async Task<List<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status)
    {
        return await _db.PurchaseOrders
            .Where(po => po.Status == status)
            .Include(po => po.Supplier)
            .Include(po => po.Items)
                .ThenInclude(poi => poi.Product)
            .AsNoTracking()
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync();
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder)
    {
        _db.PurchaseOrders.Add(purchaseOrder);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PurchaseOrder purchaseOrder)
    {
        var existing = await _db.PurchaseOrders
            .IgnoreQueryFilters()
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id);
        
        if (existing == null)
            throw new InvalidOperationException($"PurchaseOrder with Id '{purchaseOrder.Id}' not found.");

        existing.SupplierId = purchaseOrder.SupplierId;
        existing.OrderDate = purchaseOrder.OrderDate;
        existing.ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate;
        existing.Status = purchaseOrder.Status;
        existing.TotalAmount = purchaseOrder.TotalAmount;
        existing.ReferenceNo = purchaseOrder.ReferenceNo;
        existing.Notes = purchaseOrder.Notes;
        existing.IsActive = purchaseOrder.IsActive;
        existing.UpdatedAt = DateTime.Now;

        // Remove existing items
        _db.PurchaseOrderItems.RemoveRange(existing.Items);

        // Add new items
        foreach (var item in purchaseOrder.Items)
        {
            item.PurchaseOrderId = existing.Id;
            _db.PurchaseOrderItems.Add(item);
        }

        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(Guid id)
    {
        var purchaseOrder = await _db.PurchaseOrders.FirstOrDefaultAsync(po => po.Id == id);
        if (purchaseOrder != null)
        {
            purchaseOrder.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckReferenceNoExistsAsync(string referenceNo, Guid? excludeId)
    {
        if (string.IsNullOrWhiteSpace(referenceNo))
            return false;
        
        var query = _db.PurchaseOrders.AsNoTracking().Where(po => po.ReferenceNo == referenceNo);
        if (excludeId.HasValue)
            query = query.Where(po => po.Id != excludeId.Value);
        
        return await query.AnyAsync();
    }
}
