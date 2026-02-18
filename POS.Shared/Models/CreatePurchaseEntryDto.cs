namespace POS.Shared.Models;

public class CreatePurchaseEntryDto
{
    public Guid SupplierId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.Now;
    public DateTime ReceivedDate { get; set; } = DateTime.Now;
    public string? Notes { get; set; }
    
    public List<CreatePurchaseEntryItemDto> Items { get; set; } = new();
}

public class CreatePurchaseEntryItemDto
{
    public long ProductId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal MRP { get; set; }
    public decimal TaxAmount { get; set; }
}
