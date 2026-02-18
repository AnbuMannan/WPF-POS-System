namespace POS.Domain.Entities
{
    public class ReturnItem
    {
        public int ReturnItemId { get; set; }
        public decimal QuantityReturned { get; set; }
        public decimal ReturnAmount { get; set; }
        public int ReturnId { get; set; }
        public SaleReturn? SaleReturn { get; set; }
        public long SaleItemId { get; set; }
        public SaleItem? SaleItem { get; set; }

        // Enhanced fields for Sales Return module
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal RefundPrice { get; set; }
        public bool IsRestockable { get; set; } = true;
        public string? Reason { get; set; }
    }
}
