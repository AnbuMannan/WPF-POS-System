using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly IStockAdjustmentRepository _repo;
    private readonly IProductRepository _productRepo;
    private readonly IStockRepository _stockRepo;

    public StockAdjustmentService(
        IStockAdjustmentRepository repo,
        IProductRepository productRepo,
        IStockRepository stockRepo)
    {
        _repo = repo;
        _productRepo = productRepo;
        _stockRepo = stockRepo;
    }

    public async Task<List<StockAdjustmentDto>> GetAllAsync(bool includeInactive = false)
    {
        var list = await _repo.GetAllAsync(includeInactive);
        return list.Select(MapToDto).ToList();
    }

    public async Task<StockAdjustmentDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id, includeItems: true);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<StockAdjustmentDto?> GetByReferenceNoAsync(string referenceNo)
    {
        var entity = await _repo.GetByReferenceNoAsync(referenceNo);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<List<StockAdjustmentDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        var list = await _repo.GetByDateRangeAsync(fromDate, toDate);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<StockAdjustmentDto>> GetByReasonAsync(string reason)
    {
        var list = await _repo.GetByReasonAsync(reason);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<StockAdjustmentDto>> GetByStatusAsync(string status)
    {
        var list = await _repo.GetByStatusAsync(status);
        return list.Select(MapToDto).ToList();
    }

    public async Task<StockAdjustmentDto> CreateAndApproveAsync(CreateStockAdjustmentDto dto, string approvedBy)
    {
        // Validate
        var (isValid, errorMessage) = await ValidateStockAsync(dto);
        if (!isValid)
            throw new ValidationException("Stock", errorMessage ?? "Invalid stock adjustment");

        // Create entity
        var adjustment = await CreateEntityFromDto(dto);
        adjustment.Status = AdjustmentStatus.Draft;
        
        // Save
        await _repo.CreateAsync(adjustment);

        // Process with inventory updates
        await _repo.ProcessAdjustmentWithInventoryAsync(adjustment.Id);

        // Update approver info
        var processed = await _repo.GetByIdAsync(adjustment.Id, includeItems: true);
        if (processed != null)
        {
            processed.ApprovedBy = approvedBy;
            await _repo.UpdateAsync(processed);
        }

        // Return
        var result = await _repo.GetByIdAsync(adjustment.Id, includeItems: true);
        return MapToDto(result!);
    }

    public async Task<StockAdjustmentDto> CreateDraftAsync(CreateStockAdjustmentDto dto)
    {
        // Validate
        var (isValid, errorMessage) = await ValidateStockAsync(dto);
        if (!isValid)
            throw new ValidationException("Stock", errorMessage ?? "Invalid stock adjustment");

        // Create entity
        var adjustment = await CreateEntityFromDto(dto);
        adjustment.Status = AdjustmentStatus.Draft;
        
        // Save
        await _repo.CreateAsync(adjustment);

        // Return
        var result = await _repo.GetByIdAsync(adjustment.Id, includeItems: true);
        return MapToDto(result!);
    }

    public async Task<StockAdjustmentDto> ApproveAsync(Guid id, string approvedBy)
    {
        var adjustment = await _repo.GetByIdAsync(id, includeItems: true);
        if (adjustment == null)
            throw new InvalidOperationException($"StockAdjustment with Id '{id}' not found.");

        if (adjustment.Status == AdjustmentStatus.Approved)
            throw new ValidationException("Status", "This adjustment has already been approved.");

        if (adjustment.Status == AdjustmentStatus.Cancelled)
            throw new ValidationException("Status", "Cannot approve a cancelled adjustment.");

        // Re-validate stock availability before approval
        var dto = new CreateStockAdjustmentDto
        {
            Reason = adjustment.Reason,
            Items = adjustment.Items.Select(i => new CreateStockAdjustmentItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                CostPrice = i.CostPrice
            }).ToList()
        };
        
        var (isValid, errorMessage) = await ValidateStockAsync(dto);
        if (!isValid)
            throw new ValidationException("Stock", errorMessage ?? "Stock validation failed");

        // Process with inventory updates
        await _repo.ProcessAdjustmentWithInventoryAsync(id);

        // Update approver
        var processed = await _repo.GetByIdAsync(id, includeItems: true);
        if (processed != null)
        {
            processed.ApprovedBy = approvedBy;
            await _repo.UpdateAsync(processed);
        }

        var result = await _repo.GetByIdAsync(id, includeItems: true);
        return MapToDto(result!);
    }

    public async Task CancelAsync(Guid id)
    {
        var adjustment = await _repo.GetByIdAsync(id);
        if (adjustment == null)
            throw new InvalidOperationException($"StockAdjustment with Id '{id}' not found.");

        if (adjustment.Status == AdjustmentStatus.Approved)
            throw new ValidationException("Status", "Cannot cancel an approved adjustment.");

        await _repo.CancelAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var adjustment = await _repo.GetByIdAsync(id);
        if (adjustment == null)
            return false;

        if (adjustment.Status == AdjustmentStatus.Approved)
            throw new ValidationException("Status", "Cannot delete an approved adjustment.");

        await _repo.DisableAsync(id);
        return true;
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateStockAsync(CreateStockAdjustmentDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return (false, "At least one item is required.");

        // For Damage, Theft, Expiry - quantities should be negative (stock reduction)
        var isStockReduction = dto.Reason is AdjustmentReasons.Damage 
            or AdjustmentReasons.Theft 
            or AdjustmentReasons.Expiry;

        foreach (var item in dto.Items)
        {
            // Validate product exists
            var product = await _productRepo.GetByIdAsync(item.ProductId);
            if (product == null)
                return (false, $"Product with ID {item.ProductId} not found.");

            // Get current stock
            var currentStock = await _stockRepo.GetProductStockAsync(item.ProductId);

            // For stock reduction reasons, validate sufficient stock
            if (isStockReduction && item.Quantity < 0)
            {
                var absQuantity = Math.Abs(item.Quantity);
                if (absQuantity > currentStock)
                    return (false, $"Insufficient stock for '{product.Name}'. Available: {currentStock:N2}, Requested: {absQuantity:N2}");
            }
        }

        return (true, null);
    }

    private async Task<StockAdjustment> CreateEntityFromDto(CreateStockAdjustmentDto dto)
    {
        var referenceNo = await _repo.GenerateReferenceNoAsync();

        var adjustment = new StockAdjustment
        {
            Id = Guid.NewGuid(),
            ReferenceNo = referenceNo,
            AdjustmentDate = dto.AdjustmentDate,
            AdjustedBy = dto.AdjustedBy,
            Reason = dto.Reason,
            Remarks = dto.Remarks,
            TotalValue = 0,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        foreach (var itemDto in dto.Items)
        {
            var currentStock = await _stockRepo.GetProductStockAsync(itemDto.ProductId);
            
            var item = new StockAdjustmentItem
            {
                Id = Guid.NewGuid(),
                StockAdjustmentId = adjustment.Id,
                ProductId = itemDto.ProductId,
                BatchNo = itemDto.BatchNo,
                Quantity = itemDto.Quantity,
                CurrentStock = currentStock,
                CostPrice = itemDto.CostPrice,
                TotalValue = Math.Abs(itemDto.Quantity) * itemDto.CostPrice,
                Remarks = itemDto.Remarks,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            adjustment.Items.Add(item);
            adjustment.TotalValue += item.TotalValue;
        }

        return adjustment;
    }

    private static StockAdjustmentDto MapToDto(StockAdjustment entity)
    {
        return new StockAdjustmentDto
        {
            Id = entity.Id,
            ReferenceNo = entity.ReferenceNo,
            AdjustmentDate = entity.AdjustmentDate,
            AdjustedBy = entity.AdjustedBy,
            Reason = entity.Reason,
            Status = entity.Status,
            Remarks = entity.Remarks,
            ApprovedAt = entity.ApprovedAt,
            ApprovedBy = entity.ApprovedBy,
            TotalValue = entity.TotalValue,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            Items = entity.Items.Select(i => new StockAdjustmentItemDto
            {
                Id = i.Id,
                StockAdjustmentId = i.StockAdjustmentId,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                ProductSku = i.Product?.SKU,
                BatchNo = i.BatchNo,
                Quantity = i.Quantity,
                CurrentStock = i.CurrentStock,
                CostPrice = i.CostPrice,
                TotalValue = i.TotalValue,
                Remarks = i.Remarks
            }).ToList()
        };
    }
}
