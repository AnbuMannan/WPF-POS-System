namespace POS.Shared.Models;

public class PurchaseOrderItemDto
{
    public Guid PurchaseOrderItemId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
