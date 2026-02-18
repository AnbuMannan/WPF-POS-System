using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

public class PurchaseOrderItem : BaseEntity
{
    [NotMapped]
    public Guid PurchaseOrderItemId
    {
        get => Id;
        set => Id = value;
    }

    public Guid PurchaseOrderId { get; set; }
    
    [ForeignKey(nameof(PurchaseOrderId))]
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public long ProductId { get; set; }
    
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;

    [Column(TypeName = "decimal(12,3)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
}
