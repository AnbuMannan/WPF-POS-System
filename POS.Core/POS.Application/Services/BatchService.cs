using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class BatchService : IBatchService
{
    private readonly IBatchRepository _repo;

    public BatchService(IBatchRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<BatchDto>> GetAllAsync(bool includeInactive = false)
    {
        var list = await _repo.GetAllAsync(includeInactive);
        return list.Select(MapToDto).ToList();
    }

    public async Task<BatchDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<List<BatchDto>> GetByProductIdAsync(long productId)
    {
        var list = await _repo.GetByProductIdAsync(productId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<BatchDto>> GetAvailableBatchesAsync(long productId)
    {
        var list = await _repo.GetAvailableBatchesAsync(productId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<BatchDto>> GetExpiredBatchesAsync()
    {
        var list = await _repo.GetExpiredBatchesAsync();
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<BatchDto>> GetExpiringBatchesAsync(int daysThreshold = 30)
    {
        var list = await _repo.GetExpiringBatchesAsync(daysThreshold);
        return list.Select(MapToDto).ToList();
    }

    public async Task<decimal> GetTotalStockForProductAsync(long productId)
    {
        return await _repo.GetTotalStockForProductAsync(productId);
    }

    public async Task<decimal> GetAvailableStockForProductAsync(long productId)
    {
        return await _repo.GetAvailableStockForProductAsync(productId);
    }

    private static BatchDto MapToDto(Batch b) => new BatchDto
    {
        BatchId = b.Id,
        ProductId = b.ProductId,
        ProductName = b.Product?.Name,
        ProductSKU = b.Product?.SKU,
        BatchNo = b.BatchNo,
        ExpiryDate = b.ExpiryDate,
        ManufactureDate = b.ManufactureDate,
        CostPrice = b.CostPrice,
        SellingPrice = b.SellingPrice,
        MRP = b.MRP,
        ReceivedQuantity = b.ReceivedQuantity,
        CurrentQuantity = b.CurrentQuantity,
        AllocatedQuantity = b.AllocatedQuantity,
        SoldQuantity = b.SoldQuantity,
        ReturnedQuantity = b.ReturnedQuantity,
        AdjustedQuantity = b.AdjustedQuantity,
        AvailableQuantity = b.CurrentQuantity - b.AllocatedQuantity,
        PurchaseEntryId = b.PurchaseEntryId,
        SupplierId = b.SupplierId,
        SupplierName = b.Supplier?.Name,
        LocationCode = b.LocationCode,
        BinLocation = b.BinLocation,
        ReceivedDate = b.ReceivedDate,
        ReceivedBy = b.ReceivedBy,
        LastTransactionDate = b.LastTransactionDate,
        IsExpired = b.ExpiryDate.HasValue && b.ExpiryDate.Value < DateTime.Now,
        IsActive = b.IsActive,
        CreatedAt = b.CreatedAt
    };
}
