namespace POS.Shared.Models;

public class CreatePurchaseReturnDto
{
    public Guid SupplierId { get; set; }
    public Guid? PurchaseEntryId { get; set; }
    public string ReturnNo { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; } = DateTime.Now;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    
    public List<CreatePurchaseReturnItemDto> Items { get; set; } = new();
}

public class CreatePurchaseReturnItemDto
{
    public long ProductId { get; set; }
    public Guid? PurchaseEntryItemId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Reason { get; set; }
}
