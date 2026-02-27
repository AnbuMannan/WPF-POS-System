using System.ComponentModel.DataAnnotations.Schema;
using POS.Domain.Interfaces;

namespace POS.Domain.Entities;

public class PurchaseEntry : BaseEntity, IStoreEntity
{
    public int StoreCode { get; set; }
    [NotMapped]
    public Guid PurchaseEntryId
    {
        get => Id;
        set => Id = value;
    }

    public Guid SupplierId { get; set; }
    
    [ForeignKey(nameof(SupplierId))]
    public Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// Optional link to Purchase Order. Null if this is a direct purchase without PO.
    /// </summary>
    public Guid? PurchaseOrderId { get; set; }
    
    [ForeignKey(nameof(PurchaseOrderId))]
    public PurchaseOrder? PurchaseOrder { get; set; }

    public string InvoiceNo { get; set; } = string.Empty;
    
    public DateTime InvoiceDate { get; set; } = DateTime.Now;
    
    public DateTime ReceivedDate { get; set; } = DateTime.Now;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    
    public string? Notes { get; set; }
    
    /// <summary>
    /// Indicates if this entry has been processed and stock updated.
    /// </summary>
    public bool IsProcessed { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public string? ProcessedBy { get; set; }

    // Navigation property
    public ICollection<PurchaseEntryItem> Items { get; set; } = new List<PurchaseEntryItem>();
}
