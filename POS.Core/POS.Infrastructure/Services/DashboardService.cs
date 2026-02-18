using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Services;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Enums;
using POS.Infrastructure.Data;
using POS.Shared.Models;

namespace POS.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly PosDbContext _db;
    private readonly IStockRepository _stockRepository;

    public DashboardService(PosDbContext db, IStockRepository stockRepository)
    {
        _db = db;
        _stockRepository = stockRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var salesQuery = _db.Sales
            .AsNoTracking()
            .Where(s => s.CreatedAt >= start
                        && s.CreatedAt < end
                        && !s.IsDraft
                        && !s.IsHeld
                        && s.Status == SaleStatus.Completed);

        var todayTransactionCount = await salesQuery.CountAsync(cancellationToken);
        var todaySalesAmount = await salesQuery.SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

        var lowStockProducts = await _stockRepository.GetLowStockProductsAsync();
        var lowStockItemCount = lowStockProducts.Count();

        var pendingOrdersCount = await _db.PurchaseOrders
            .AsNoTracking()
            .CountAsync(po => po.Status == PurchaseOrderStatus.Pending, cancellationToken);

        var recentSales = await salesQuery
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .Select(s => new DashboardRecentSaleDto
            {
                SaleId = s.SaleId,
                BillNumber = s.BillNumber,
                CreatedAt = s.CreatedAt,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                TotalAmount = s.TotalAmount,
                Status = s.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            TodaySalesAmount = todaySalesAmount,
            TodayTransactionCount = todayTransactionCount,
            LowStockItemCount = lowStockItemCount,
            PendingOrdersCount = pendingOrdersCount,
            RecentSales = recentSales
        };
    }
}
