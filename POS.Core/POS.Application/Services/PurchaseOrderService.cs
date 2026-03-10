using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Shared.Models;

namespace POS.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly ISupplierRepository _supplierRepo;
    private readonly IProductRepository _productRepo;

    public PurchaseOrderService(
        IPurchaseOrderRepository repo,
        ISupplierRepository supplierRepo,
        IProductRepository productRepo)
    {
        _repo = repo;
        _supplierRepo = supplierRepo;
        _productRepo = productRepo;
    }

    public async Task<List<PurchaseOrderDto>> GetAllAsync(bool includeInactive = false)
    {
        var list = await _repo.GetAllAsync(includeInactive);
        return list.Select(MapToDto).ToList();
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id, includeItems: true);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<List<PurchaseOrderDto>> GetPendingOrdersBySupplierAsync(Guid supplierId)
    {
        var list = await _repo.GetPendingOrdersBySuppliersAsync(supplierId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<PurchaseOrderDto>> GetByStatusAsync(PurchaseOrderStatus status)
    {
        var list = await _repo.GetByStatusAsync(status);
        return list.Select(MapToDto).ToList();
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto)
    {
        // Validate Supplier exists
        var supplier = await _supplierRepo.GetByIdAsync(dto.SupplierId);
        if (supplier == null || !supplier.IsActive)
            throw new ValidationException("SupplierId", "Invalid or inactive supplier.");

        // Validate items
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ValidationException("Items", "At least one item is required.");

        // Validate all products exist
        foreach (var item in dto.Items)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId);
            if (product == null || !product.IsActive)
                throw new ValidationException("ProductId", $"Product {item.ProductId} not found or inactive.");

            if (item.Quantity <= 0)
                throw new ValidationException("Quantity", "Quantity must be greater than zero.");
            
            if (item.UnitPrice < 0)
                throw new ValidationException("UnitPrice", "Unit price cannot be negative.");
        }

        // Calculate totals
        var entity = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            SupplierId = dto.SupplierId,
            OrderDate = dto.OrderDate,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            ReferenceNo = dto.ReferenceNo,
            Notes = dto.Notes,
            Status = PurchaseOrderStatus.Draft,
            CreatedAt = DateTime.Now,
            IsActive = true,
            Items = new List<PurchaseOrderItem>()
        };

        foreach (var itemDto in dto.Items)
        {
            var itemTotal = (itemDto.Quantity * itemDto.UnitPrice) + itemDto.TaxAmount;
            var item = new PurchaseOrderItem
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = entity.Id,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                TaxAmount = itemDto.TaxAmount,
                TotalAmount = itemTotal,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            entity.Items.Add(item);
        }

        entity.TotalAmount = entity.Items.Sum(i => i.TotalAmount);

        await _repo.AddAsync(entity);
        
        // Reload with navigation properties
        var created = await _repo.GetByIdAsync(entity.Id, includeItems: true);
        return MapToDto(created!);
    }

    public async Task<PurchaseOrderDto> UpdateAsync(Guid id, CreatePurchaseOrderDto dto)
    {
        var existing = await _repo.GetByIdAsync(id, includeItems: true);
        if (existing == null)
            throw new InvalidOperationException($"PurchaseOrder with Id '{id}' not found.");

        // Can only update Draft orders
        if (existing.Status != PurchaseOrderStatus.Draft)
            throw new ValidationException("Status", "Only draft orders can be updated.");

        // Validate Supplier exists
        var supplier = await _supplierRepo.GetByIdAsync(dto.SupplierId);
        if (supplier == null || !supplier.IsActive)
            throw new ValidationException("SupplierId", "Invalid or inactive supplier.");

        // Validate items
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ValidationException("Items", "At least one item is required.");

        // Validate all products exist
        foreach (var item in dto.Items)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId);
            if (product == null || !product.IsActive)
                throw new ValidationException("ProductId", $"Product {item.ProductId} not found or inactive.");

            if (item.Quantity <= 0)
                throw new ValidationException("Quantity", "Quantity must be greater than zero.");
            
            if (item.UnitPrice < 0)
                throw new ValidationException("UnitPrice", "Unit price cannot be negative.");
        }

        existing.SupplierId = dto.SupplierId;
        existing.OrderDate = dto.OrderDate;
        existing.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
        existing.ReferenceNo = dto.ReferenceNo;
        existing.Notes = dto.Notes;
        existing.UpdatedAt = DateTime.Now;

        // Rebuild items
        existing.Items.Clear();
        foreach (var itemDto in dto.Items)
        {
            var itemTotal = (itemDto.Quantity * itemDto.UnitPrice) + itemDto.TaxAmount;
            var item = new PurchaseOrderItem
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = existing.Id,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                TaxAmount = itemDto.TaxAmount,
                TotalAmount = itemTotal,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            existing.Items.Add(item);
        }

        existing.TotalAmount = existing.Items.Sum(i => i.TotalAmount);

        await _repo.UpdateAsync(existing);
        
        // Reload with navigation properties
        var updated = await _repo.GetByIdAsync(existing.Id, includeItems: true);
        return MapToDto(updated!);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, PurchaseOrderStatus status)
    {
        var existing = await _repo.GetByIdAsync(id, includeItems: false);
        if (existing == null)
            return false;

        existing.Status = status;
        existing.UpdatedAt = DateTime.Now;
        await _repo.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DisableAsync(Guid id)
    {
        await _repo.DisableAsync(id);
        return true;
    }

    public async Task<bool> CheckReferenceNoExistsAsync(string referenceNo, Guid? excludeId)
        => await _repo.CheckReferenceNoExistsAsync(referenceNo, excludeId);

    private static PurchaseOrderDto MapToDto(PurchaseOrder e) => new PurchaseOrderDto
    {
        PurchaseOrderId = e.Id,
        SupplierId = e.SupplierId,
        SupplierName = e.Supplier?.Name,
        SupplierCode = e.Supplier?.Code,
        OrderDate = e.OrderDate,
        ExpectedDeliveryDate = e.ExpectedDeliveryDate,
        Status = (POS.Shared.Enums.PurchaseOrderStatus)e.Status,
        TotalAmount = e.TotalAmount,
        ReferenceNo = e.ReferenceNo,
        Notes = e.Notes,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Items = e.Items?.Select(i => new PurchaseOrderItemDto
        {
            PurchaseOrderItemId = i.Id,
            PurchaseOrderId = i.PurchaseOrderId,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name,
            ProductSKU = i.Product?.SKU,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TaxAmount = i.TaxAmount,
            TotalAmount = i.TotalAmount
        }).ToList() ?? new List<PurchaseOrderItemDto>()
    };
}
