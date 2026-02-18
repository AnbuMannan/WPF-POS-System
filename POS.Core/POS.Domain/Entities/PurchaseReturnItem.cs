using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

public class PurchaseReturnItem : BaseEntity
{
    [NotMapped]
    public Guid PurchaseReturnItemId
    {
        get => Id;
        set => Id = value;
    }

    public Guid PurchaseReturnId { get; set; }
    
    [ForeignKey(nameof(PurchaseReturnId))]
    public PurchaseReturn PurchaseReturn { get; set; } = null!;

    public long ProductId { get; set; }
    
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Optional reference to the original PurchaseEntryItem being returned
    /// </summary>
    public Guid? PurchaseEntryItemId { get; set; }

    public string? BatchNo { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [Column(TypeName = "decimal(12,3)")]
    public decimal Quantity { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    public string? Reason { get; set; }
}
