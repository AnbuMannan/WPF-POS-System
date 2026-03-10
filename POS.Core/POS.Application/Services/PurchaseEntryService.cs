using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Shared.Models;

namespace POS.Application.Services;

public class PurchaseEntryService : IPurchaseEntryService
{
    private readonly IPurchaseEntryRepository _repo;
    private readonly ISupplierRepository _supplierRepo;
    private readonly IProductRepository _productRepo;
    private readonly IPurchaseOrderRepository _purchaseOrderRepo;
    private readonly ISupplierTransactionService _transactionService;

    public PurchaseEntryService(
        IPurchaseEntryRepository repo,
        ISupplierRepository supplierRepo,
        IProductRepository productRepo,
        IPurchaseOrderRepository purchaseOrderRepo,
        ISupplierTransactionService transactionService)
    {
        _repo = repo;
        _supplierRepo = supplierRepo;
        _productRepo = productRepo;
        _purchaseOrderRepo = purchaseOrderRepo;
        _transactionService = transactionService;
    }

    public async Task<List<PurchaseEntryDto>> GetAllAsync(bool includeInactive = false)
    {
        var list = await _repo.GetAllAsync(includeInactive);
        return list.Select(MapToDto).ToList();
    }

    public async Task<PurchaseEntryDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id, includeItems: true);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<List<PurchaseEntryDto>> GetBySupplierAsync(Guid supplierId)
    {
        var list = await _repo.GetBySuppliersAsync(supplierId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<PurchaseEntryDto>> GetUnprocessedAsync()
    {
        var list = await _repo.GetUnprocessedAsync();
        return list.Select(MapToDto).ToList();
    }

    public async Task<PurchaseEntryDto> CreateAsync(CreatePurchaseEntryDto dto, int storeCode)
    {
        // Validate Supplier exists
        var supplier = await _supplierRepo.GetByIdAsync(dto.SupplierId);
        if (supplier == null || !supplier.IsActive)
            throw new ValidationException("SupplierId", "Invalid or inactive supplier.");

        // Validate PurchaseOrder if provided
        if (dto.PurchaseOrderId.HasValue)
        {
            var po = await _purchaseOrderRepo.GetByIdAsync(dto.PurchaseOrderId.Value, includeItems: false);
            if (po == null)
                throw new ValidationException("PurchaseOrderId", "Purchase order not found.");
        }

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
            
            if (item.CostPrice < 0)
                throw new ValidationException("CostPrice", "Cost price cannot be negative.");
        }

        // Calculate totals
        var entity = new PurchaseEntry
        {
            Id = Guid.NewGuid(),
            StoreCode = storeCode,
            SupplierId = dto.SupplierId,
            PurchaseOrderId = dto.PurchaseOrderId,
            InvoiceNo = dto.InvoiceNo,
            InvoiceDate = dto.InvoiceDate,
            ReceivedDate = dto.ReceivedDate,
            Notes = dto.Notes,
            IsProcessed = false,
            CreatedAt = DateTime.Now,
            IsActive = true,
            Items = new List<PurchaseEntryItem>()
        };

        decimal totalTax = 0;
        foreach (var itemDto in dto.Items)
        {
            var itemTotal = (itemDto.Quantity * itemDto.CostPrice) + itemDto.TaxAmount;
            totalTax += itemDto.TaxAmount;
            
            var item = new PurchaseEntryItem
            {
                Id = Guid.NewGuid(),
                PurchaseEntryId = entity.Id,
                ProductId = itemDto.ProductId,
                BatchNo = itemDto.BatchNo,
                ExpiryDate = itemDto.ExpiryDate,
                Quantity = itemDto.Quantity,
                CostPrice = itemDto.CostPrice,
                SellingPrice = itemDto.SellingPrice,
                MRP = itemDto.MRP,
                TaxAmount = itemDto.TaxAmount,
                TotalAmount = itemTotal,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            entity.Items.Add(item);
        }

        entity.TaxAmount = totalTax;
        entity.TotalAmount = entity.Items.Sum(i => i.TotalAmount);

        await _repo.AddAsync(entity);
        
        // Reload with navigation properties
        var created = await _repo.GetByIdAsync(entity.Id, includeItems: true);
        return MapToDto(created!);
    }

    public async Task<PurchaseEntryDto> UpdateAsync(Guid id, CreatePurchaseEntryDto dto, int storeCode)
    {
        var existing = await _repo.GetByIdAsync(id, includeItems: true);
        if (existing == null)
            throw new InvalidOperationException($"PurchaseEntry with Id '{id}' not found.");

        // Cannot update if already processed
        if (existing.IsProcessed)
            throw new ValidationException("IsProcessed", "Cannot update a processed purchase entry.");

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
            
            if (item.CostPrice < 0)
                throw new ValidationException("CostPrice", "Cost price cannot be negative.");
        }

        existing.StoreCode = storeCode;
        existing.SupplierId = dto.SupplierId;
        existing.PurchaseOrderId = dto.PurchaseOrderId;
        existing.InvoiceNo = dto.InvoiceNo;
        existing.InvoiceDate = dto.InvoiceDate;
        existing.ReceivedDate = dto.ReceivedDate;
        existing.Notes = dto.Notes;
        existing.UpdatedAt = DateTime.Now;

        // Rebuild items
        existing.Items.Clear();
        decimal totalTax = 0;
        
        foreach (var itemDto in dto.Items)
        {
            var itemTotal = (itemDto.Quantity * itemDto.CostPrice) + itemDto.TaxAmount;
            totalTax += itemDto.TaxAmount;
            
            var item = new PurchaseEntryItem
            {
                Id = Guid.NewGuid(),
                PurchaseEntryId = existing.Id,
                ProductId = itemDto.ProductId,
                BatchNo = itemDto.BatchNo,
                ExpiryDate = itemDto.ExpiryDate,
                Quantity = itemDto.Quantity,
                CostPrice = itemDto.CostPrice,
                SellingPrice = itemDto.SellingPrice,
                MRP = itemDto.MRP,
                TaxAmount = itemDto.TaxAmount,
                TotalAmount = itemTotal,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            existing.Items.Add(item);
        }

        existing.TaxAmount = totalTax;
        existing.TotalAmount = existing.Items.Sum(i => i.TotalAmount);

        await _repo.UpdateAsync(existing);
        
        // Reload with navigation properties
        var updated = await _repo.GetByIdAsync(existing.Id, includeItems: true);
        return MapToDto(updated!);
    }

    /// <summary>
    /// CRITICAL LOGIC: Process the purchase entry and update inventory
    /// </summary>
    public async Task<PurchaseEntryDto> ProcessEntryAsync(Guid id, int storeCode, bool updateProductPrices = true)
    {
        var entry = await _repo.GetByIdAsync(id, includeItems: true);
        if (entry == null)
            throw new InvalidOperationException($"PurchaseEntry with Id '{id}' not found.");

        if (entry.IsProcessed)
            throw new ValidationException("IsProcessed", "This purchase entry has already been processed.");

        // Process the entry with inventory updates (handled at repository level with transaction)
        await _repo.ProcessEntryWithInventoryUpdateAsync(id, updateProductPrices, storeCode);

        // Record supplier transaction (Credit entry - amount owed to supplier)
        await _transactionService.RecordPurchaseAsync(
            entry.SupplierId,
            entry.Id,
            entry.InvoiceNo,
            entry.TotalAmount,
            storeCode,
            $"Purchase Entry: {entry.InvoiceNo}"
        );

        // Reload with navigation properties
        var processed = await _repo.GetByIdAsync(entry.Id, includeItems: true);
        return MapToDto(processed!);
    }

    public async Task<bool> DisableAsync(Guid id)
    {
        var entry = await _repo.GetByIdAsync(id, includeItems: false);
        if (entry == null)
            return false;

        // Cannot delete if already processed (inventory already updated)
        if (entry.IsProcessed)
            throw new ValidationException("IsProcessed", "Cannot delete a processed purchase entry. It has already updated inventory.");

        await _repo.DisableAsync(id);
        return true;
    }

    public async Task<bool> CheckInvoiceNoExistsAsync(string invoiceNo, Guid? excludeId)
        => await _repo.CheckInvoiceNoExistsAsync(invoiceNo, excludeId);

    // ================= MAPPING =================

    private static PurchaseEntryDto MapToDto(PurchaseEntry e) => new PurchaseEntryDto
    {
        PurchaseEntryId = e.Id,
        SupplierId = e.SupplierId,
        SupplierName = e.Supplier?.Name,
        SupplierCode = e.Supplier?.Code,
        PurchaseOrderId = e.PurchaseOrderId,
        PurchaseOrderReferenceNo = e.PurchaseOrder?.ReferenceNo,
        InvoiceNo = e.InvoiceNo,
        InvoiceDate = e.InvoiceDate,
        ReceivedDate = e.ReceivedDate,
        TotalAmount = e.TotalAmount,
        TaxAmount = e.TaxAmount,
        Notes = e.Notes,
        IsProcessed = e.IsProcessed,
        ProcessedAt = e.ProcessedAt,
        ProcessedBy = e.ProcessedBy,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Items = e.Items?.Select(i => new PurchaseEntryItemDto
        {
            PurchaseEntryItemId = i.Id,
            PurchaseEntryId = i.PurchaseEntryId,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name,
            ProductSKU = i.Product?.SKU,
            BatchNo = i.BatchNo,
            ExpiryDate = i.ExpiryDate,
            Quantity = i.Quantity,
            CostPrice = i.CostPrice,
            SellingPrice = i.SellingPrice,
            MRP = i.MRP,
            TaxAmount = i.TaxAmount,
            TotalAmount = i.TotalAmount
        }).ToList() ?? new List<PurchaseEntryItemDto>()
    };
}
