namespace POS.Shared.Models;

public class PurchaseReturnItemDto
{
    public Guid Id { get; set; }
    public Guid PurchaseReturnId { get; set; }
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public Guid? PurchaseEntryItemId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Reason { get; set; }
}
