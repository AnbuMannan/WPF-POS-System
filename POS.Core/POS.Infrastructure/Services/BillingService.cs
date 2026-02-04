using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Data;
using POS.Shared.Models;

namespace POS.Infrastructure.Services;

public class BillingService : IBillingService
{
    private readonly PosDbContext _db;

    public BillingService(PosDbContext db)
    {
        _db = db;
    }

    public async Task<ReceiptDto> CreateSaleAsync(CreateSaleDto dto, string userId, CancellationToken cancellationToken = default)
    {
        int defaultTaxProfileId = 0;
        if (dto.Items.Any(i => i.TaxProfileId <= 0))
        {
            defaultTaxProfileId = await _db.TaxProfiles.AsNoTracking().OrderBy(t => t.TaxProfileId).Select(t => t.TaxProfileId).FirstOrDefaultAsync(cancellationToken);
            if (defaultTaxProfileId == 0)
                throw new InvalidOperationException("At least one Tax Profile must exist. Please add a tax profile in Admin before creating sales.");
        }

        Guid defaultUomId = await _db.Uoms.AsNoTracking().OrderBy(u => u.Name).Select(u => u.Id).FirstOrDefaultAsync(cancellationToken);
        if (defaultUomId == Guid.Empty)
            throw new InvalidOperationException("At least one UOM must exist. Please add a unit of measure in Admin before creating sales.");

        var sale = new Sale
        {
            BillNumber = dto.BillNumber ?? $"INV{DateTime.Now:yyyyMMddHHmmss}",
            SaleType = SaleType.Regular,
            Status = SaleStatus.Completed,
            CustomerId = dto.CustomerId,
            Subtotal = dto.Subtotal,
            DiscountAmount = dto.DiscountAmount,
            TotalTax = dto.TaxAmount,
            TotalAmount = dto.GrandTotal,
            RoundOff = Math.Round(dto.GrandTotal) - dto.GrandTotal,
            PaymentStatus = PaymentStatus.Completed,
            IsDraft = false,
            IsHeld = false,
            LoyaltyPointsEarned = 0,
            LoyaltyPointsRedeemed = 0,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        int lineNum = 1;
        foreach (var item in dto.Items)
        {
            int taxProfileId = item.TaxProfileId > 0 ? item.TaxProfileId : defaultTaxProfileId;
            sale.SaleItems.Add(new SaleItem
            {
                ProductId = GuidToLong(item.ProductId),
                ProductName = item.ProductName ?? "Product",
                SKU = item.SKU ?? "",
                UnitName = item.Unit ?? "PCS",
                UomId = defaultUomId,
                HSNCode = item.HSNCode,
                LineNumber = lineNum++,
                Quantity = item.Quantity,
                ActualPrice = item.UnitPrice,
                SellingPrice = item.UnitPrice,
                DiscountPercent = item.DiscountPercent,
                DiscountAmount = item.DiscountAmount,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                Subtotal = item.Quantity * item.UnitPrice - item.DiscountAmount,
                TotalAmount = item.TotalAmount,
                CGST = item.TaxAmount / 2,
                SGST = item.TaxAmount / 2,
                IGST = 0,
                Cess = 0,
                TaxProfileId = taxProfileId,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var pay in dto.Payments)
        {
            var method = ParsePaymentMethod(pay.PaymentMethod);
            sale.Payments.Add(new Payment
            {
                PaymentMethod = method,
                Amount = pay.Amount,
                Status = PaymentStatus.Completed,
                CreatedAt = pay.PaymentDate != default ? pay.PaymentDate : DateTime.UtcNow
            });
        }

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync(cancellationToken);

        return BuildReceiptDto(sale, dto, userId);
    }

    public Task<string> GenerateBillNumberAsync(CancellationToken cancellationToken = default)
    {
        var billNumber = $"INV{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
        return Task.FromResult(billNumber);
    }

    private static long GuidToLong(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToInt64(bytes, 0);
    }

    private static PaymentMethod ParsePaymentMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method)) return PaymentMethod.Other;
        return method.Trim().ToUpperInvariant() switch
        {
            "CASH" => PaymentMethod.Cash,
            "CARD" => PaymentMethod.Card,
            "UPI" => PaymentMethod.UPI,
            "GIFTCARD" => PaymentMethod.GiftCard,
            _ => PaymentMethod.Other
        };
    }

    private static ReceiptDto BuildReceiptDto(Sale sale, CreateSaleDto dto, string userId)
    {
        var items = sale.SaleItems.Select(x => new ReceiptItemDto
        {
            ProductName = x.ProductName,
            Quantity = x.Quantity,
            Unit = x.UnitName,
            UnitPrice = x.ActualPrice,
            TotalAmount = x.TotalAmount,
            TotalPrice = x.TotalAmount,
            Discount = x.DiscountAmount > 0 ? x.DiscountAmount : null,
            HSNCode = x.HSNCode
        }).ToList();

        var payments = sale.Payments.Select(p => new ReceiptPaymentDto
        {
            Method = p.PaymentMethod.ToString(),
            Amount = p.Amount
        }).ToList();

        decimal totalTax = sale.SaleItems.Sum(x => x.TaxAmount);
        return new ReceiptDto
        {
            SaleId = (int)sale.SaleId,
            BillNumber = sale.BillNumber,
            ReceiptNumber = sale.BillNumber,
            SaleDate = sale.CreatedAt,
            TransactionDate = sale.CreatedAt,
            StoreName = "My Awesome POS Store",
            StoreAddress = "123 Main St, Anytown, State 12345",
            StoreGSTIN = "GSTIN1234567890",
            Items = items,
            SubTotal = sale.Subtotal,
            Discount = sale.DiscountAmount,
            TaxAmount = totalTax,
            GrandTotal = sale.TotalAmount,
            TotalAmount = sale.TotalAmount,
            AmountPaid = sale.Payments.Sum(p => p.Amount),
            Payments = payments,
            CGST = totalTax / 2,
            SGST = totalTax / 2,
            IGST = 0,
            RoundOff = sale.RoundOff,
            TotalItemCount = items.Count,
            TotalQuantity = items.Sum(x => x.Quantity),
            CashierName = userId,
            ThankYouMessage = "Thank you for shopping with us!",
            FooterMessage = "Come back again soon!"
        };
    }
}
