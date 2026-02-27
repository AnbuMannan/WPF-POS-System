using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Shared.Models;

namespace POS.Application.Services;

public class QuotationService : IQuotationService
{
    private readonly IQuotationRepository _repo;
    private readonly IProductRepository _productRepo;
    private readonly IBillingService _billingService;

    public QuotationService(
        IQuotationRepository repo,
        IProductRepository productRepo,
        IBillingService billingService)
    {
        _repo = repo;
        _productRepo = productRepo;
        _billingService = billingService;
    }

    public async Task<List<QuotationDto>> GetAllAsync(bool includeInactive = false)
    {
        var quotations = await _repo.GetAllAsync(includeInactive);
        return quotations.Select(MapToDto).ToList();
    }

    public async Task<QuotationDto?> GetByIdAsync(Guid id)
    {
        var q = await _repo.GetByIdAsync(id);
        return q == null ? null : MapToDto(q);
    }

    public async Task<QuotationDto> CreateAsync(CreateQuotationDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ValidationException("Items", "At least one item is required.");

        var quotationNumber = await _repo.GenerateQuotationNumberAsync();

        var quotation = new Quotation
        {
            Id = Guid.NewGuid(),
            QuotationNumber = quotationNumber,
            QuotationDate = DateTime.Now,
            ValidUntil = dto.ValidUntil,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            Status = QuotationStatus.Open,
            Notes = dto.Notes,
            TermsAndConditions = dto.TermsAndConditions,
            CreatedBy = "System",
            IsActive = true,
            CreatedAt = DateTime.Now,
            Items = dto.Items.Select(i => new QuotationItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                SKU = i.SKU,
                HSNCode = i.HSNCode,
                Quantity = i.Quantity,
                UnitName = i.UnitName,
                UnitPrice = i.UnitPrice,
                DiscountPercent = i.DiscountPercent,
                DiscountAmount = i.DiscountAmount,
                TaxRate = i.TaxRate,
                TaxAmount = i.TaxAmount,
                TotalAmount = i.TotalAmount,
                IsActive = true,
                CreatedAt = DateTime.Now
            }).ToList()
        };

        // Calculate totals
        quotation.Subtotal = quotation.Items.Sum(i => i.Quantity * i.UnitPrice);
        quotation.DiscountAmount = quotation.Items.Sum(i => i.DiscountAmount);
        quotation.TaxAmount = quotation.Items.Sum(i => i.TaxAmount);
        quotation.TotalAmount = quotation.Items.Sum(i => i.TotalAmount);

        var created = await _repo.AddAsync(quotation);
        return MapToDto(created);
    }

    public async Task<QuotationDto> UpdateAsync(Guid id, CreateQuotationDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            throw new ValidationException("Quotation", "Quotation not found.");

        if (existing.Status == QuotationStatus.Converted)
            throw new ValidationException("Quotation", "Cannot edit a converted quotation.");

        existing.CustomerId = dto.CustomerId;
        existing.CustomerName = dto.CustomerName;
        existing.CustomerPhone = dto.CustomerPhone;
        existing.ValidUntil = dto.ValidUntil;
        existing.Notes = dto.Notes;
        existing.TermsAndConditions = dto.TermsAndConditions;
        existing.UpdatedAt = DateTime.Now;

        // Replace items
        existing.Items.Clear();
        foreach (var i in dto.Items)
        {
            existing.Items.Add(new QuotationItem
            {
                Id = Guid.NewGuid(),
                QuotationId = id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                SKU = i.SKU,
                HSNCode = i.HSNCode,
                Quantity = i.Quantity,
                UnitName = i.UnitName,
                UnitPrice = i.UnitPrice,
                DiscountPercent = i.DiscountPercent,
                DiscountAmount = i.DiscountAmount,
                TaxRate = i.TaxRate,
                TaxAmount = i.TaxAmount,
                TotalAmount = i.TotalAmount,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
        }

        existing.Subtotal = existing.Items.Sum(i => i.Quantity * i.UnitPrice);
        existing.DiscountAmount = existing.Items.Sum(i => i.DiscountAmount);
        existing.TaxAmount = existing.Items.Sum(i => i.TaxAmount);
        existing.TotalAmount = existing.Items.Sum(i => i.TotalAmount);

        var updated = await _repo.UpdateAsync(existing);
        return MapToDto(updated);
    }

    public async Task DisableAsync(Guid id)
    {
        await _repo.DisableAsync(id);
    }

    public async Task<long> ConvertToSaleAsync(Guid quotationId)
    {
        var quotation = await _repo.GetByIdAsync(quotationId);
        if (quotation == null)
            throw new ValidationException("Quotation", "Quotation not found.");

        if (quotation.Status == QuotationStatus.Converted)
            throw new ValidationException("Quotation", "This quotation has already been converted to a sale.");

        if (quotation.Status == QuotationStatus.Cancelled)
            throw new ValidationException("Quotation", "Cannot convert a cancelled quotation.");

        // Create sale via billing service
        var createSaleDto = new CreateSaleDto
        {
            CustomerId = quotation.CustomerId,
            Subtotal = quotation.Subtotal,
            DiscountAmount = quotation.DiscountAmount,
            TaxAmount = quotation.TaxAmount,
            GrandTotal = quotation.TotalAmount,
            Items = quotation.Items.Select(qi => new SaleItemDto
            {
                ProductId = new Guid(qi.ProductId.ToString().PadLeft(32, '0')), // Convert long to Guid for DTO compatibility
                ProductName = qi.ProductName,
                SKU = qi.SKU,
                Quantity = qi.Quantity,
                Unit = qi.UnitName,
                UnitPrice = qi.UnitPrice,
                DiscountPercent = qi.DiscountPercent,
                DiscountAmount = qi.DiscountAmount,
                TaxRate = qi.TaxRate,
                TaxAmount = qi.TaxAmount,
                TotalAmount = qi.TotalAmount,
                HSNCode = qi.HSNCode
            }).ToList()
        };

        // Note: The actual sale creation should go through the billing flow
        // For now, mark the quotation as converted
        quotation.Status = QuotationStatus.Converted;
        quotation.ConvertedAt = DateTime.Now;
        quotation.ConvertedBy = "System";
        await _repo.UpdateStatusAsync(quotationId, "Converted");

        // Return 0 - the caller should create the sale through the billing screen
        return 0;
    }

    private static QuotationDto MapToDto(Quotation q)
    {
        return new QuotationDto
        {
            Id = q.Id,
            QuotationNumber = q.QuotationNumber,
            QuotationDate = q.QuotationDate,
            ValidUntil = q.ValidUntil,
            CustomerId = q.CustomerId,
            CustomerName = q.CustomerName ?? q.Customer?.Name,
            CustomerPhone = q.CustomerPhone ?? q.Customer?.Phone,
            Status = q.Status.ToString(),
            Subtotal = q.Subtotal,
            DiscountAmount = q.DiscountAmount,
            TaxAmount = q.TaxAmount,
            TotalAmount = q.TotalAmount,
            Notes = q.Notes,
            TermsAndConditions = q.TermsAndConditions,
            ConvertedSaleId = q.ConvertedSaleId,
            ConvertedAt = q.ConvertedAt,
            ConvertedBy = q.ConvertedBy,
            CreatedBy = q.CreatedBy,
            IsActive = q.IsActive,
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt,
            Items = q.Items.Select(qi => new QuotationItemDto
            {
                Id = qi.Id,
                QuotationId = qi.QuotationId,
                ProductId = qi.ProductId,
                ProductName = qi.ProductName,
                SKU = qi.SKU,
                HSNCode = qi.HSNCode,
                Quantity = qi.Quantity,
                UnitName = qi.UnitName,
                UnitPrice = qi.UnitPrice,
                DiscountPercent = qi.DiscountPercent,
                DiscountAmount = qi.DiscountAmount,
                TaxRate = qi.TaxRate,
                TaxAmount = qi.TaxAmount,
                TotalAmount = qi.TotalAmount
            }).ToList()
        };
    }
}
