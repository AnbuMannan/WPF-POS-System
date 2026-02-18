using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class SaleReturnService : ISaleReturnService
{
    private readonly ISaleReturnRepository _repo;
    private readonly ICustomerPaymentService _customerPaymentService;

    public SaleReturnService(ISaleReturnRepository repo, ICustomerPaymentService customerPaymentService)
    {
        _repo = repo;
        _customerPaymentService = customerPaymentService;
    }

    public async Task<List<SaleReturnDto>> GetAllAsync()
    {
        var returns = await _repo.GetAllAsync();
        return returns.Select(MapToDto).ToList();
    }

    public async Task<SaleReturnDto?> GetByIdAsync(int id)
    {
        var sr = await _repo.GetByIdAsync(id);
        return sr == null ? null : MapToDto(sr);
    }

    public async Task<SaleInvoiceForReturnDto?> LookupInvoiceAsync(string billNumber)
    {
        if (string.IsNullOrWhiteSpace(billNumber))
            return null;

        var sale = await _repo.GetSaleByBillNumberAsync(billNumber.Trim());
        if (sale == null) return null;

        return await MapSaleForReturn(sale);
    }

    public async Task<SaleInvoiceForReturnDto?> LookupInvoiceBySaleIdAsync(long saleId)
    {
        var sale = await _repo.GetSaleWithItemsAsync(saleId);
        if (sale == null) return null;

        return await MapSaleForReturn(sale);
    }

    public async Task<SaleReturnDto> CreateReturnAsync(CreateSaleReturnDto dto)
    {
        // Validate sale exists
        var sale = await _repo.GetSaleWithItemsAsync(dto.OriginalSaleId);
        if (sale == null)
            throw new ValidationException("Sale", "Sale not found.");

        // Validate items
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ValidationException("Items", "At least one item is required for return.");

        foreach (var item in dto.Items)
        {
            var saleItem = sale.SaleItems.FirstOrDefault(si => si.SaleItemId == item.SaleItemId);
            if (saleItem == null)
                throw new ValidationException("SaleItem", $"Sale item {item.SaleItemId} not found in the invoice.");

            // Validate return quantity <= sold quantity - already returned
            var alreadyReturned = await _repo.GetAlreadyReturnedQuantityAsync(item.SaleItemId);
            var maxReturn = saleItem.Quantity - alreadyReturned;
            if (item.QuantityReturned > maxReturn)
                throw new ValidationException("Quantity", $"Return quantity ({item.QuantityReturned}) exceeds available quantity ({maxReturn}) for {saleItem.ProductName}.");

            if (item.QuantityReturned <= 0)
                throw new ValidationException("Quantity", $"Return quantity must be greater than 0 for {saleItem.ProductName}.");
        }

        // Generate return number
        var returnNumber = await _repo.GenerateReturnNumberAsync();

        var saleReturn = new SaleReturn
        {
            ReturnNumber = returnNumber,
            ReturnType = "Return",
            OriginalSaleId = dto.OriginalSaleId,
            Reason = dto.Reason,
            RefundMode = dto.RefundMode ?? "Cash",
            CustomerId = sale.CustomerId,
            ReturnDate = DateTime.Now,
            Status = "Draft",
            IsProcessed = false,
            CreatedBy = "System",
            CreatedAt = DateTime.Now,
            TotalReturnAmount = dto.Items.Sum(i => i.QuantityReturned * i.RefundPrice),
            RefundAmount = dto.Items.Sum(i => i.QuantityReturned * i.RefundPrice),
            ReturnItems = dto.Items.Select(i =>
            {
                var saleItem = sale.SaleItems.First(si => si.SaleItemId == i.SaleItemId);
                return new ReturnItem
                {
                    SaleItemId = i.SaleItemId,
                    ProductId = i.ProductId > 0 ? i.ProductId : saleItem.ProductId,
                    ProductName = saleItem.ProductName,
                    QuantityReturned = i.QuantityReturned,
                    RefundPrice = i.RefundPrice,
                    ReturnAmount = i.QuantityReturned * i.RefundPrice,
                    IsRestockable = i.IsRestockable,
                    Reason = i.Reason
                };
            }).ToList()
        };

        var created = await _repo.CreateAsync(saleReturn);
        return MapToDto(created);
    }

    public async Task<SaleReturnDto> ProcessReturnAsync(int returnId)
    {
        var sr = await _repo.GetByIdAsync(returnId);
        if (sr == null) throw new ValidationException("Return", "Sale return not found.");
        if (sr.IsProcessed) throw new ValidationException("Return", "This return has already been processed.");

        // Process inventory update (restock + stock ledger)
        await _repo.ProcessReturnWithInventoryAsync(returnId);

        // Record customer transaction if credit note and customer exists
        if (sr.CustomerId.HasValue && sr.RefundMode == "CreditNote")
        {
            await _customerPaymentService.RecordReturnTransactionAsync(
                sr.CustomerId.Value,
                sr.ReturnId,
                sr.ReturnNumber,
                sr.RefundAmount,
                sr.RefundMode);
        }

        // Re-fetch to get updated state
        var updated = await _repo.GetByIdAsync(returnId);
        return MapToDto(updated!);
    }

    private async Task<SaleInvoiceForReturnDto> MapSaleForReturn(Sale sale)
    {
        var dto = new SaleInvoiceForReturnDto
        {
            SaleId = sale.SaleId,
            BillNumber = sale.BillNumber,
            InvoiceNumber = sale.InvoiceNumber,
            CreatedAt = sale.CreatedAt,
            CustomerName = sale.CustomerName,
            CustomerId = sale.CustomerId,
            TotalAmount = sale.TotalAmount,
            Items = new List<SaleItemForReturnDto>()
        };

        foreach (var item in sale.SaleItems)
        {
            var alreadyReturned = await _repo.GetAlreadyReturnedQuantityAsync(item.SaleItemId);
            dto.Items.Add(new SaleItemForReturnDto
            {
                SaleItemId = item.SaleItemId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                SKU = item.SKU,
                Quantity = item.Quantity,
                SellingPrice = item.SellingPrice,
                TotalAmount = item.TotalAmount,
                AlreadyReturned = alreadyReturned,
                MaxReturnQuantity = item.Quantity - alreadyReturned,
                IsReturned = item.IsReturned
            });
        }

        return dto;
    }

    private static SaleReturnDto MapToDto(SaleReturn sr)
    {
        return new SaleReturnDto
        {
            ReturnId = sr.ReturnId,
            ReturnNumber = sr.ReturnNumber,
            ReturnType = sr.ReturnType,
            TotalReturnAmount = sr.TotalReturnAmount,
            RefundAmount = sr.RefundAmount,
            Reason = sr.Reason,
            OriginalSaleId = sr.OriginalSaleId,
            OriginalBillNumber = sr.OriginalSale?.BillNumber,
            CustomerName = sr.Customer?.Name ?? sr.OriginalSale?.CustomerName,
            CustomerId = sr.CustomerId,
            ReturnDate = sr.ReturnDate,
            RefundMode = sr.RefundMode,
            Status = sr.Status,
            IsProcessed = sr.IsProcessed,
            CreatedBy = sr.CreatedBy,
            CreatedAt = sr.CreatedAt,
            Items = sr.ReturnItems.Select(ri => new SaleReturnItemDto
            {
                ReturnItemId = ri.ReturnItemId,
                SaleItemId = ri.SaleItemId,
                ProductId = ri.ProductId,
                ProductName = ri.ProductName,
                SKU = ri.SaleItem?.SKU,
                QuantityReturned = ri.QuantityReturned,
                RefundPrice = ri.RefundPrice,
                ReturnAmount = ri.ReturnAmount,
                IsRestockable = ri.IsRestockable,
                Reason = ri.Reason
            }).ToList()
        };
    }
}

// ValidationException is defined in POS.Application.Exceptions
