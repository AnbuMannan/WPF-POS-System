namespace POS.Domain.Entities
{
    public class SaleItem
    {
        public long SaleItemId { get; set; }
        public long SaleId { get; set; }
        public Sale? Sale { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? HSNCode { get; set; }
        public int LineNumber { get; set; }
        public decimal Quantity { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public Guid UomId { get; set; }
        public int TaxProfileId { get; set; }
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsFreeItem { get; set; }
        public bool IsReturned { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
