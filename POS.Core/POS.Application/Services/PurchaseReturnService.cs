using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly IPurchaseReturnRepository _repo;
    private readonly ISupplierRepository _supplierRepo;
    private readonly IProductRepository _productRepo;
    private readonly IPurchaseEntryRepository _purchaseEntryRepo;
    private readonly ISupplierTransactionService _transactionService;

    public PurchaseReturnService(
        IPurchaseReturnRepository repo,
        ISupplierRepository supplierRepo,
        IProductRepository productRepo,
        IPurchaseEntryRepository purchaseEntryRepo,
        ISupplierTransactionService transactionService)
    {
        _repo = repo;
        _supplierRepo = supplierRepo;
        _productRepo = productRepo;
        _purchaseEntryRepo = purchaseEntryRepo;
        _transactionService = transactionService;
    }

    public async Task<List<PurchaseReturnDto>> GetAllAsync(bool includeInactive = false)
    {
        var list = await _repo.GetAllAsync(includeInactive);
        return list.Select(MapToDto).ToList();
    }

    public async Task<PurchaseReturnDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id, includeItems: true);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<List<PurchaseReturnDto>> GetBySupplierAsync(Guid supplierId)
    {
        var list = await _repo.GetBySupplierAsync(supplierId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<PurchaseReturnDto>> GetByPurchaseEntryIdAsync(Guid purchaseEntryId)
    {
        var list = await _repo.GetByPurchaseEntryIdAsync(purchaseEntryId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<PurchaseReturnDto>> GetUnprocessedAsync()
    {
        var list = await _repo.GetUnprocessedAsync();
        return list.Select(MapToDto).ToList();
    }

    public async Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto dto, int storeCode)
    {
        // Validate Supplier exists
        var supplier = await _supplierRepo.GetByIdAsync(dto.SupplierId);
        if (supplier == null || !supplier.IsActive)
            throw new ValidationException("SupplierId", "Invalid or inactive supplier.");

        // Validate PurchaseEntry if provided
        if (dto.PurchaseEntryId.HasValue)
        {
            var pe = await _purchaseEntryRepo.GetByIdAsync(dto.PurchaseEntryId.Value, includeItems: false);
            if (pe == null)
                throw new ValidationException("PurchaseEntryId", "Purchase entry not found.");
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
                throw new ValidationException("Quantity", "Return quantity must be greater than zero.");
            
            if (item.UnitPrice < 0)
                throw new ValidationException("UnitPrice", "Unit price cannot be negative.");
        }

        // Check if return number already exists
        if (!string.IsNullOrWhiteSpace(dto.ReturnNo))
        {
            var exists = await _repo.CheckReturnNoExistsAsync(dto.ReturnNo, null);
            if (exists)
                throw new ValidationException("ReturnNo", "Return number already exists.");
        }

        // Calculate totals
        var entity = new PurchaseReturn
        {
            Id = Guid.NewGuid(),
            StoreCode = storeCode,
            SupplierId = dto.SupplierId,
            PurchaseEntryId = dto.PurchaseEntryId,
            ReturnNo = dto.ReturnNo,
            ReturnDate = dto.ReturnDate,
            Reason = dto.Reason,
            Notes = dto.Notes,
            Status = "Draft",
            IsProcessed = false,
            CreatedAt = DateTime.Now,
            IsActive = true,
            Items = new List<PurchaseReturnItem>()
        };

        decimal totalAmount = 0;
        decimal totalTax = 0;

        foreach (var itemDto in dto.Items)
        {
            var item = new PurchaseReturnItem
            {
                Id = Guid.NewGuid(),
                PurchaseReturnId = entity.Id,
                ProductId = itemDto.ProductId,
                PurchaseEntryItemId = itemDto.PurchaseEntryItemId,
                BatchNo = itemDto.BatchNo,
                ExpiryDate = itemDto.ExpiryDate,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                TaxAmount = itemDto.TaxAmount,
                TotalAmount = itemDto.TotalAmount,
                Reason = itemDto.Reason,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            totalAmount += item.TotalAmount;
            totalTax += item.TaxAmount;

            entity.Items.Add(item);
        }

        entity.TotalAmount = totalAmount;
        entity.TaxAmount = totalTax;

        await _repo.AddAsync(entity);
        return MapToDto(entity);
    }

    public async Task<PurchaseReturnDto> UpdateAsync(Guid id, CreatePurchaseReturnDto dto, int storeCode)
    {
        var entity = await _repo.GetByIdAsync(id, includeItems: true);
        if (entity == null)
            throw new InvalidOperationException($"PurchaseReturn with Id '{id}' not found.");

        if (entity.IsProcessed)
            throw new ValidationException("IsProcessed", "Cannot update a processed purchase return.");

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
                throw new ValidationException("Quantity", "Return quantity must be greater than zero.");
        }

        // Check if return number already exists (excluding current)
        if (!string.IsNullOrWhiteSpace(dto.ReturnNo))
        {
            var exists = await _repo.CheckReturnNoExistsAsync(dto.ReturnNo, id);
            if (exists)
                throw new ValidationException("ReturnNo", "Return number already exists.");
        }

        // Update header
        entity.StoreCode = storeCode;
        entity.SupplierId = dto.SupplierId;
        entity.PurchaseEntryId = dto.PurchaseEntryId;
        entity.ReturnNo = dto.ReturnNo;
        entity.ReturnDate = dto.ReturnDate;
        entity.Reason = dto.Reason;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.Now;

        // Clear existing items and add new ones
        entity.Items.Clear();

        decimal totalAmount = 0;
        decimal totalTax = 0;

        foreach (var itemDto in dto.Items)
        {
            var item = new PurchaseReturnItem
            {
                Id = Guid.NewGuid(),
                PurchaseReturnId = entity.Id,
                ProductId = itemDto.ProductId,
                PurchaseEntryItemId = itemDto.PurchaseEntryItemId,
                BatchNo = itemDto.BatchNo,
                ExpiryDate = itemDto.ExpiryDate,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                TaxAmount = itemDto.TaxAmount,
                TotalAmount = itemDto.TotalAmount,
                Reason = itemDto.Reason,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            totalAmount += item.TotalAmount;
            totalTax += item.TaxAmount;

            entity.Items.Add(item);
        }

        entity.TotalAmount = totalAmount;
        entity.TaxAmount = totalTax;

        await _repo.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<PurchaseReturnDto> ProcessReturnAsync(Guid id, int storeCode)
    {
        var entity = await _repo.GetByIdAsync(id, includeItems: true);
        if (entity == null)
            throw new InvalidOperationException($"PurchaseReturn with Id '{id}' not found.");

        if (entity.IsProcessed)
            throw new ValidationException("IsProcessed", "Purchase return has already been processed.");

        if (!entity.Items.Any())
            throw new ValidationException("Items", "Cannot process a return with no items.");

        // Process the return with inventory update (atomic transaction in repository)
        await _repo.ProcessReturnWithInventoryUpdateAsync(id, storeCode);

        // Record supplier transaction (Debit entry - reduces amount owed to supplier)
        await _transactionService.RecordPurchaseReturnAsync(
            entity.SupplierId,
            entity.Id,
            entity.ReturnNo ?? string.Empty,
            entity.TotalAmount,
            storeCode,
            $"Purchase Return: {entity.ReturnNo}"
        );

        // Reload entity to get updated values
        entity = await _repo.GetByIdAsync(id, includeItems: true);
        return MapToDto(entity!);
    }

    public async Task<bool> DisableAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id, includeItems: false);
        if (entity == null)
            throw new InvalidOperationException($"PurchaseReturn with Id '{id}' not found.");

        if (entity.IsProcessed)
            throw new ValidationException("IsProcessed", "Cannot disable a processed purchase return.");

        await _repo.DisableAsync(id);
        return true;
    }

    public async Task<bool> CheckReturnNoExistsAsync(string returnNo, Guid? excludeId)
    {
        return await _repo.CheckReturnNoExistsAsync(returnNo, excludeId);
    }

    #region Mapping

    private PurchaseReturnDto MapToDto(PurchaseReturn entity)
    {
        return new PurchaseReturnDto
        {
            Id = entity.Id,
            SupplierId = entity.SupplierId,
            SupplierName = entity.Supplier?.Name ?? string.Empty,
            PurchaseEntryId = entity.PurchaseEntryId,
            PurchaseEntryInvoiceNo = entity.PurchaseEntry?.InvoiceNo,
            ReturnNo = entity.ReturnNo,
            ReturnDate = entity.ReturnDate,
            TotalAmount = entity.TotalAmount,
            TaxAmount = entity.TaxAmount,
            Reason = entity.Reason,
            Notes = entity.Notes,
            Status = entity.Status,
            IsProcessed = entity.IsProcessed,
            ProcessedAt = entity.ProcessedAt,
            ProcessedBy = entity.ProcessedBy,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Items = entity.Items.Select(MapItemToDto).ToList()
        };
    }

    private PurchaseReturnItemDto MapItemToDto(PurchaseReturnItem item)
    {
        return new PurchaseReturnItemDto
        {
            Id = item.Id,
            PurchaseReturnId = item.PurchaseReturnId,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            ProductCode = item.Product?.SKU ?? string.Empty,
            PurchaseEntryItemId = item.PurchaseEntryItemId,
            BatchNo = item.BatchNo,
            ExpiryDate = item.ExpiryDate,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TaxAmount = item.TaxAmount,
            TotalAmount = item.TotalAmount,
            Reason = item.Reason
        };
    }

    #endregion
}
