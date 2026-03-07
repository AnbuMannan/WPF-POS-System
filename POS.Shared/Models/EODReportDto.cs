namespace POS.Shared.Models
{
    /// <summary>Summary of a single sale for EOD top-sales list.</summary>
    public class EODSaleSummaryDto
    {
        public string BillNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Product with quantity sold and revenue for EOD top-selling products.</summary>
    public class EODTopProductDto
    {
        public long ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class EODReportDto
    {
        public DateTime Date { get; set; }
        public int SaleCount { get; set; }
        public decimal TotalSales { get; set; }
        public decimal SubtotalSum { get; set; }
        public decimal TaxSum { get; set; }
        public decimal DiscountSum { get; set; }

        /// <summary>Total revenue (same as TotalSales), alias for clarity.</summary>
        public decimal TotalRevenue => TotalSales;

        // Tax breakdown
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalIGST { get; set; }

        // Discounts & returns
        public decimal TotalDiscountsGiven => DiscountSum;
        public int TotalReturnsCount { get; set; }
        public decimal TotalRefunds { get; set; }

        // Payment method breakdown (key: enum name e.g. "Cash", "Card")
        public Dictionary<string, decimal> PaymentBreakdown { get; set; } = new();

        // Cash reconciliation support
        public decimal CashSalesAmount { get; set; }
        public decimal CashRefundAmount { get; set; }
        public decimal TotalExpenses { get; set; }

        // Top sales (e.g. top 10 by amount)
        public List<EODSaleSummaryDto> TopSales { get; set; } = new();

        // Top selling products (e.g. top 10 by quantity)
        public List<EODTopProductDto> TopSellingProducts { get; set; } = new();
    }
}
