namespace POS.Shared.Models;

public class PurchaseEntryDto
{
    public Guid PurchaseEntryId { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderReferenceNo { get; set; }
    public string InvoiceNo { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime ReceivedDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public List<PurchaseEntryItemDto> Items { get; set; } = new();
}
