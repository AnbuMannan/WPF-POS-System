using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Services;
using POS.Domain.Enums;
using POS.Infrastructure.Data;
using POS.Shared.Models;

namespace POS.Infrastructure.Services;

public class EODReportService : IEODReportService
{
    private readonly PosDbContext _db;

    public EODReportService(PosDbContext db)
    {
        _db = db;
    }

    public async Task<EODReportDto> GetEODReportAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var salesQuery = _db.Sales
            .Where(s => s.CreatedAt >= start && s.CreatedAt < end
                && !s.IsDraft && !s.IsHeld
                && s.Status == SaleStatus.Completed);

        var saleCount = await salesQuery.CountAsync(cancellationToken);
        var totals = await salesQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                SubtotalSum = g.Sum(s => s.Subtotal),
                TaxSum = g.Sum(s => s.TotalTax),
                DiscountSum = g.Sum(s => s.DiscountAmount),
                TotalSales = g.Sum(s => s.TotalAmount),
                TotalCGST = g.Sum(s => s.CGST),
                TotalSGST = g.Sum(s => s.SGST),
                TotalIGST = g.Sum(s => s.IGST),
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Payment breakdown: from Payments for sales in this date range
        var paymentBreakdown = await _db.Payments
            .Where(p => _db.Sales.Any(s =>
                s.SaleId == p.SaleId
                && s.CreatedAt >= start && s.CreatedAt < end
                && !s.IsDraft && !s.IsHeld
                && s.Status == SaleStatus.Completed))
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Total = g.Sum(p => p.Amount) })
            .ToListAsync(cancellationToken);

        var breakdownDict = new Dictionary<string, decimal>();
        foreach (var item in paymentBreakdown)
            breakdownDict[item.Method.ToString()] = item.Total;

        decimal cashSales = breakdownDict.GetValueOrDefault(nameof(PaymentMethod.Cash), 0);

        // Returns: SaleReturns created on this date (refunds against sales)
        var returnsQuery = _db.Returns
            .Where(r => r.CreatedAt >= start && r.CreatedAt < end);
        var returnsCount = await returnsQuery.CountAsync(cancellationToken);
        var totalRefunds = await returnsQuery.SumAsync(r => r.RefundAmount, cancellationToken);
        // Cash refunds: if we don't track payment method on returns, use 0 or assume proportional; here we use 0 for simplicity
        decimal cashRefunds = 0;

        // Top 10 sales by total amount
        var topSales = await salesQuery
            .OrderByDescending(s => s.TotalAmount)
            .Take(10)
            .Select(s => new EODSaleSummaryDto
            {
                BillNumber = s.BillNumber,
                TotalAmount = s.TotalAmount,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Top 10 selling products by quantity (from SaleItems of sales in date range)
        var topProducts = await _db.SaleItems
            .Where(si => _db.Sales.Any(s =>
                s.SaleId == si.SaleId
                && s.CreatedAt >= start && s.CreatedAt < end
                && !s.IsDraft && !s.IsHeld
                && s.Status == SaleStatus.Completed))
            .GroupBy(si => new { si.ProductId, si.ProductName, si.SKU })
            .Select(g => new
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.ProductName,
                SKU = g.Key.SKU,
                QuantitySold = g.Sum(si => si.Quantity),
                Revenue = g.Sum(si => si.TotalAmount)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(10)
            .ToListAsync(cancellationToken);

        var topProductDtos = topProducts.Select(p => new EODTopProductDto
        {
            ProductId = p.ProductId,
            Name = p.Name ?? "",
            SKU = p.SKU ?? "",
            QuantitySold = p.QuantitySold,
            Revenue = p.Revenue
        }).ToList();

        return new EODReportDto
        {
            Date = date.Date,
            SaleCount = saleCount,
            SubtotalSum = totals?.SubtotalSum ?? 0,
            TaxSum = totals?.TaxSum ?? 0,
            DiscountSum = totals?.DiscountSum ?? 0,
            TotalSales = totals?.TotalSales ?? 0,
            TotalCGST = totals?.TotalCGST ?? 0,
            TotalSGST = totals?.TotalSGST ?? 0,
            TotalIGST = totals?.TotalIGST ?? 0,
            PaymentBreakdown = breakdownDict,
            CashSalesAmount = cashSales,
            CashRefundAmount = cashRefunds,
            TotalReturnsCount = returnsCount,
            TotalRefunds = totalRefunds,
            TopSales = topSales,
            TopSellingProducts = topProductDtos
        };
    }

    public async Task CloseDayReportAsync(DateTime date, string? lockedBy = null, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        var by = lockedBy ?? "System";

        var sales = await _db.Sales
            .Where(s => s.CreatedAt >= start && s.CreatedAt < end
                && !s.IsDraft && !s.IsHeld
                && s.Status == SaleStatus.Completed
                && !s.IsLocked)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var s in sales)
        {
            s.IsLocked = true;
            s.LockedAt = now;
            s.LockedBy = by;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
