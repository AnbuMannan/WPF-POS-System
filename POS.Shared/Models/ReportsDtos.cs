namespace POS.Shared.Models;

public class SalesSummaryReportRow
{
    public DateTime Date { get; set; }
    public int InvoiceCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ItemWiseSalesRow
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public decimal QuantitySold { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ProfitLossReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalCogs { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal ProfitLoss { get; set; }
}

public class LowStockItemRow
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal AvailableStock { get; set; }
    public decimal ReorderLevel { get; set; }
}

