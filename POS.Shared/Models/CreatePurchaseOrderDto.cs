namespace POS.Shared.Models;

public class CreatePurchaseOrderDto
{
    public Guid SupplierId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemDto
{
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
}
