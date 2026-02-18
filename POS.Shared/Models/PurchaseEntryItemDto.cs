namespace POS.Shared.Models;

public class PurchaseEntryItemDto
{
    public Guid PurchaseEntryItemId { get; set; }
    public Guid PurchaseEntryId { get; set; }
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal MRP { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
