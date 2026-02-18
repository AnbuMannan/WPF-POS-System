using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

public class PurchaseEntryItem : BaseEntity
{
    [NotMapped]
    public Guid PurchaseEntryItemId
    {
        get => Id;
        set => Id = value;
    }

    public Guid PurchaseEntryId { get; set; }
    
    [ForeignKey(nameof(PurchaseEntryId))]
    public PurchaseEntry PurchaseEntry { get; set; } = null!;

    public long ProductId { get; set; }
    
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;

    public string? BatchNo { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [Column(TypeName = "decimal(12,3)")]
    public decimal Quantity { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal SellingPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MRP { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
}
