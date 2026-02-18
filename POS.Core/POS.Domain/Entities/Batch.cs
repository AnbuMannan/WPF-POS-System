using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

/// <summary>
/// Batch entity for batch-level stock management (market-standard POS)
/// Stock is maintained at batch level - all transactions affect batch quantities
/// </summary>
public class Batch : BaseEntity
{
    public Guid BatchId => Id;
    
    // Product Information
    public long ProductId { get; set; }
    public virtual Product? Product { get; set; }
    
    // Batch Details
    public string BatchNo { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    
    // Pricing (captured at time of purchase)
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal MRP { get; set; }
    
    // Stock Quantities
    public decimal ReceivedQuantity { get; set; } // Initial quantity received
    public decimal CurrentQuantity { get; set; }  // Current available quantity
    public decimal AllocatedQuantity { get; set; } // Reserved for orders/holds
    public decimal SoldQuantity { get; set; }      // Total sold
    public decimal ReturnedQuantity { get; set; }  // Total returned
    public decimal AdjustedQuantity { get; set; }  // Total adjusted (wastage, damage, etc.)
    
    // Source Information
    public Guid? PurchaseEntryId { get; set; }
    public virtual PurchaseEntry? PurchaseEntry { get; set; }
    
    public Guid? PurchaseEntryItemId { get; set; }
    public virtual PurchaseEntryItem? PurchaseEntryItem { get; set; }
    
    // Supplier Information
    public Guid SupplierId { get; set; }
    public virtual Supplier? Supplier { get; set; }
    
    // Location (for multi-location support)
    public string? LocationCode { get; set; }
    public string? BinLocation { get; set; }
    
    // Status
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
    public bool IsLowStock => CurrentQuantity <= ReorderLevel;
    public decimal ReorderLevel { get; set; } = 0;
    
    // Audit Fields
    public DateTime ReceivedDate { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    
    // Computed Properties
    public decimal AvailableQuantity => CurrentQuantity - AllocatedQuantity;
}
