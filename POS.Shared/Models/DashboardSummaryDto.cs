namespace POS.Shared.Models;

public class DashboardSummaryDto
{
    public decimal TodaySalesAmount { get; set; }
    public int TodayTransactionCount { get; set; }
    public int LowStockItemCount { get; set; }
    public int PendingOrdersCount { get; set; }
    public List<DashboardRecentSaleDto> RecentSales { get; set; } = new();
}

public class DashboardRecentSaleDto
{
    public long SaleId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

