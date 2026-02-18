using System.ComponentModel.DataAnnotations.Schema;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    [NotMapped]
    public Guid PurchaseOrderId
    {
        get => Id;
        set => Id = value;
    }

    public Guid SupplierId { get; set; }
    
    [ForeignKey(nameof(SupplierId))]
    public Supplier Supplier { get; set; } = null!;

    public DateTime OrderDate { get; set; } = DateTime.Now;
    
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    public string? ReferenceNo { get; set; }
    
    public string? Notes { get; set; }

    // Navigation property
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
