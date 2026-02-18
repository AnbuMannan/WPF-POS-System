using POS.Shared.Enums;

namespace POS.Shared.Models;

public class PurchaseOrderDto
{
    public Guid PurchaseOrderId { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}
