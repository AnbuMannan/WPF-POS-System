using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

public class PurchaseReturn : BaseEntity
{
    [NotMapped]
    public Guid PurchaseReturnId
    {
        get => Id;
        set => Id = value;
    }

    public Guid SupplierId { get; set; }
    
    [ForeignKey(nameof(SupplierId))]
    public Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// Optional link to Purchase Entry (GRN). Null if returning without reference to specific GRN.
    /// </summary>
    public Guid? PurchaseEntryId { get; set; }
    
    [ForeignKey(nameof(PurchaseEntryId))]
    public PurchaseEntry? PurchaseEntry { get; set; }

    public string ReturnNo { get; set; } = string.Empty;
    
    public DateTime ReturnDate { get; set; } = DateTime.Now;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    
    public string? Reason { get; set; }
    
    public string? Notes { get; set; }
    
    /// <summary>
    /// Status: Draft, Processed, Cancelled
    /// </summary>
    public string Status { get; set; } = "Draft";
    
    /// <summary>
    /// Indicates if this return has been processed and stock/ledger updated.
    /// </summary>
    public bool IsProcessed { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public string? ProcessedBy { get; set; }

    // Navigation property
    public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
}
